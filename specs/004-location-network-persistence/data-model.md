# Data Model: Location, Network Persistence & Movement

## Entities

### Location

Represents a discovered network node the player can visit.

| Field | Type | Description |
|-------|------|-------------|
| `Id` | `string` | Unique integer seed stored as string (e.g., `"1"`). Acts as stable key in `_locationCache`. |
| `Name` | `string` | Human-readable label derived deterministically: `"node_" + (int.Parse(Id) * 77 % 1000)`. **New in v2.** |
| `ConfigId` | `int` | Reference to location configuration profile used for procedural generation. |
| `IsVisited` | `bool` | **Removed from movement logic** (I-1). Retained on the model only if other systems still reference it; otherwise deprecate. |
| `Networks` | `List<Network>` | Discovered networks at this location. Empty until the player runs `scan` here. |
| `LastVisitedUtc` | `long` | UTC ticks of last visit; used by `OfflineIncomeCalculator`. |

**Identity rule**: `Id` is unique across all locations in a session. `Name` is unique for seeds 0–999 (bijection via `gcd(77, 1000) = 1`).

**State transitions**:

```
[Generated] → [Visited/Scanned] → [Forgotten (partial: networks removed)] → [Empty (no networks)]
```

Locations are never fully deleted from `_locationCache` — forgetting all networks leaves the location with an empty `Networks` list, still addressable by name.

---

### Network

Belongs to exactly one `Location`.

| Field | Type | Description |
|-------|------|-------------|
| `SSID` | `string` | Network name. Not guaranteed unique across locations — disambiguation needed when same SSID appears at multiple locations. |
| `SecurityLevel` | `SecurityLevel` (enum) | e.g., `None`, `WEP`, `WPA2`. |
| `IsHacked` | `bool` | Whether the player has cracked this network. |
| `Devices` | `List<Device>` | Devices discovered within this network via `scan ip`/`scan mac`. |

**Invariant**: `Devices` is empty until the player scans within the network.

---

### Device

Belongs to exactly one `Network` (and transitively to one `Location`).

| Field | Type | Description |
|-------|------|-------------|
| `IpAddress` | `string` | Unique within a network; used as lookup key by `ForgetDevice()`. |
| `MacAddress` | `string` | Hardware identifier. |
| `HasFirewall` | `bool` | Whether the device's firewall is enabled. |
| `ActiveMalware` | `Malware?` | `null` if clean; non-null if the player has injected malware. |
| `InstalledAtUtcTicks` | `long` | UTC ticks when malware was installed; used by `OfflineIncomeCalculator`. |

**State transitions**:

```
[Discovered] → [Firewall Disabled] → [Infected] → [Forgotten (removed from Network.Devices)]
```

---

### SaveData (v2)

Root save object, serialised with `JsonUtility`.

| Field | Type | Description |
|-------|------|-------------|
| `Version` | `int` | Schema version. `1` = legacy (no Name, no CurrentLocationName). `2` = current. |
| `Player` | `PlayerSaveData` | Player stats and inventory. |
| `Locations` | `List<LocationSaveData>` | All discovered locations. |
| `CurrentLocationName` | `string` | Name of the location the player was at on last save. **New in v2.** Falls back to first location's name if null (migration). |
| `NextLocationId` | `int` | Monotonically increasing counter for generating new location IDs. |

---

### LocationSaveData (v2)

| Field | Type | Description |
|-------|------|-------------|
| `Id` | `string` | Integer seed as string. |
| `Name` | `string` | Derived from `int.Parse(Id) * 77 % 1000`. **New in v2.** |
| `ConfigId` | `int` | Location config profile. |
| `IsVisited` | `bool` | See Location notes above. |
| `Networks` | `List<NetworkSaveData>` | Serialised network list. |

**Migration (v1 → v2)**:
- If `Name` is null or empty: back-fill `"node_" + (int.Parse(Id) * 77 % 1000)`.
- If `CurrentLocationName` is null: set to first entry's name (or `"node_77"` if list empty).
- Set `Version = 2` and persist immediately.

---

### ForgetResult

Value type returned by `LocationService.ForgetNetwork()` and `LocationService.ForgetDevice()`.

| Field | Type | Description |
|-------|------|-------------|
| `Success` | `bool` | `true` if the entity was found and removed. |
| `Message` | `string` | User-facing confirmation or error string. |
| `IpsRemoved` | `int` | Count of infected IPs whose income was stopped (0 for `ForgetDevice` that targets a clean device, 1 if infected). |

**Static factory**:

```
ForgetResult.Ambiguous(string[] locationNames)
  → Success = false
  → Message = "Network found at multiple locations: ..."
  → IpsRemoved = 0
```

Callers that receive an `Ambiguous` result render the location names as a numbered selection list.

---

### SelectionState (TerminalController internal)

Nullable struct held by `TerminalController`. Present only while a selection is active.

| Field | Type | Description |
|-------|------|-------------|
| `Options` | `string[]` | Caller-supplied option labels (without "Cancel"). "Cancel" is always appended internally as the last entry. |
| `HighlightIndex` | `int` | Zero-based index of the currently highlighted row (0 = first option, `Options.Length` = Cancel). |
| `OnSelected` | `Action<int>` | Callback. Index into `Options` (0-based) on confirm; `-1` on Cancel or free-text abort. |

---

## Relationships

```
SaveData
  └── List<LocationSaveData>  (1..*)
        └── List<NetworkSaveData>  (0..*)
              └── List<DeviceSaveData>  (0..*)

LocationService._locationCache : Dictionary<string, Location>
  key = Location.Id
  └── Location
        └── List<Network>
              └── List<Device>

LocationService._knownLocationsSnapshot : List<Location>
  Rebuilt on every cache mutation (add/remove).
  Returned by GetAllKnownLocations() — zero allocation in hot path.
```

---

## Validation Rules

- `Location.Name` must not be null or empty after `FromSaveData()` completes.
- `SaveData.CurrentLocationName` must resolve to an existing `Location.Name` in `_locationCache`; fall back to first location if not found.
- `ForgetNetwork(ssid, locationName)` with a non-null `locationName` must only remove from that specific location — never from others even if SSID matches.
- `ForgetDevice(ip)` searches all locations; if the IP appears in more than one location (not expected but possible), removes the first match found and reports `IpsRemoved = 1`.
- Forgetting a network or device does NOT roll back offline income already credited; only future ticks are affected.
