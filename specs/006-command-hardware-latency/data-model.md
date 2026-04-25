# Data Model: Command Hardware Latency

**Feature**: `specs/006-command-hardware-latency` | **Date**: 2026-04-25

---

## Entity 1 — PlayerHardware (fields on `Player`)

New persistent integer fields added to `Assets/Scripts/Models/Player.cs`:

| Field | Type | Default | Range | Description |
|-------|------|---------|-------|-------------|
| `CpuTier` | `int` | 1 | 1–5 | Processing power tier. 1 = base laptop; 5 = server rack. |
| `InternetTier` | `int` | 1 | 1–5 | Connection bandwidth tier. 1 = DSL; 5 = dedicated line. |
| `GpuTier` | `int` | 0 | 0–3 | GPU tier. 0 = none; 3 = compute cluster. |

**JsonUtility note**: `int` fields missing from a save default to `0`. The v2→v3 migration corrects `CpuTier==0→1` and `InternetTier==0→1`. `GpuTier==0` is the correct default (no GPU).

**Persistence**: Fields serialise automatically as part of `Player` which is already a `[System.Serializable]` nested in `SaveData`. No structural change to `SaveData` or `LocationSaveData` required.

**Validation rules**:
- On purchase: `newTier > currentTier` (can only upgrade, not downgrade).
- `CpuTier` and `InternetTier` clamp to [1, 5]; `GpuTier` clamps to [0, 3].

**Removed field**: `CommandSpeedUpgrade` (float) — deleted from `Player`. All callers migrated to `CommandLatencyService`.

---

## Entity 2 — DeviceHardware (fields on `Device` and `DeviceSaveData`)

New fields on `Assets/Scripts/Models/Device.cs`:

| Field | Type | Default | Range | Description |
|-------|------|---------|-------|-------------|
| `CpuTier` | `int` | 1 | 1–5 | Target-device processing speed. Affects inject and firewall resistance. |
| `BandwidthTier` | `int` | 1 | 1–5 | Target-device network bandwidth. Affects ls/copy speed and scan overhead. |

New corresponding fields on `DeviceSaveData` (in `Assets/Scripts/Models/SaveData.cs`):

| Field | Type | Description |
|-------|------|-------------|
| `CpuTier` | `int` | Serialised device CPU tier. |
| `BandwidthTier` | `int` | Serialised device bandwidth tier. |

**Generation**: Assigned by `LocationService.GenerateLocation(int seed)` using the location's seeded PRNG. Both fields use `rng.Next(1, 6)` called after all existing device property generation, preserving existing seed sequences.

**Persistence**: Serialised through `LocationService.ToSaveData()` → `DeviceSaveData`; restored in `LocationService.RestoreFromSave()`.

**Display**: `scan ip` (or `scan mac`) output includes `CPU Tier: N | BW Tier: N` once `IsScanned == true` (FR-016).

**State transition**:
```
Device created          →  CpuTier and BandwidthTier set from PRNG (1–5)
DeviceSaveData saved    →  CpuTier, BandwidthTier serialised
DeviceSaveData loaded   →  CpuTier, BandwidthTier restored
v2→v3 migration         →  DeviceSaveData with 0-values set to 1 (neutral baseline)
```

---

## Entity 3 — CommandProfile (data constants in `CommandLatencyService`)

Design data describing the timing characteristics of each command verb. Stored as parallel arrays and constants inside `CommandLatencyService` (no ScriptableObject — values are code-side balance constants).

| Command | Base time (s) | Max cap (s) | Player modifiers | Target modifiers |
|---------|--------------|------------|-----------------|-----------------|
| `crack WEP` | 3.0 | 10.0 | CPU, GPU | — |
| `crack WPA` | 6.0 | 20.0 | CPU, GPU | — |
| `crack WPA2` | 10.0 | 30.0 | CPU, GPU | — |
| `scan` (area) | 1.5 | 8.0 | Internet | — |
| `scan ip/mac` | 2.0 | 8.0 | Internet | Target BW overhead |
| `inject` | 4.0 | 15.0 | CPU, Internet | Target CPU resist., Target BW resist. |
| `firewall` | 2.5 | 12.0 | Internet | Target CPU resist. |
| `ls` | 0.5 | 8.0 | Internet | Target BW speedup |
| `copy` | 3.0 | 15.0 | Internet | Target BW speedup, File size |
| `show` | — (instant) | — | — | — |
| `help` | — (instant) | — | — | — |
| `forget` | — (instant) | — | — | — |
| `move` | — (instant) | — | — | — |

**File-size scaling for `copy`**: `baseTime = BaseTimeCopy + FileSizeBytes / ReferenceSizeBytes × CopyScaleFactor`. `ReferenceSizeBytes = 10_000_000` (10 MB), `CopyScaleFactor = 2.0f`. Result is then divided by playerSpeedup and multiplied by targetResistance before capping.

**SC-004 check**: Slowest (crack WPA2, 10 s) ÷ fastest non-instant (ls, 0.5 s) = 20× at base tier. ≥ 3× required. ✓

---

## Entity 4 — HardwareShopItem (fields on `StoreItemSO` and `StoreItem`)

Extension to `Assets/Scripts/Data/StoreItemSO.cs` and `Assets/Scripts/Models/StoreItem.cs`:

New fields on `StoreItemSO`:

| Field | Type | Description |
|-------|------|-------------|
| `HasHardwareUpgrade` | `bool` | Guards the hardware tier block (pattern: same as `HasToolUnlock`). |
| `HardwareStatAffected` | `HardwareStat` | Which stat this item upgrades (CPU, Internet, GPU). |
| `HardwareTierGranted` | `int` | The tier the player reaches on purchase. |

Corresponding fields mirrored in `StoreItem` runtime model. `StoreItemSO.ToModel()` maps them across.

**New enum** `HardwareStat` in `Assets/Scripts/Models/HardwareStat.cs`:
```
public enum HardwareStat { CPU = 0, Internet = 1, GPU = 2 }
```

**Effect application** in `StoreService.ApplyEffect()`: new branch sets `_player.CpuTier`, `_player.InternetTier`, or `_player.GpuTier` = `item.HardwareTierGranted`. Guard: only apply if `HardwareTierGranted > current tier`.

**Planned assets** (`Assets/Resources/StoreItems/`):

| Asset ID | Display name | Stat | Tier | Price (BTC) | Affected commands |
|----------|-------------|------|------|------------|------------------|
| `hw-cpu-2` | Dual-Core CPU | CPU | 2 | 0.0050 | crack, inject |
| `hw-cpu-3` | Quad-Core CPU | CPU | 3 | 0.0150 | crack, inject |
| `hw-cpu-4` | Workstation CPU | CPU | 4 | 0.0500 | crack, inject |
| `hw-cpu-5` | Server Rack CPU | CPU | 5 | 0.1500 | crack, inject |
| `hw-net-2` | Cable Internet | Internet | 2 | 0.0030 | scan, inject, firewall, ls, copy |
| `hw-net-3` | Fibre Lite | Internet | 3 | 0.0100 | scan, inject, firewall, ls, copy |
| `hw-net-4` | Full Fibre | Internet | 4 | 0.0300 | scan, inject, firewall, ls, copy |
| `hw-net-5` | Dedicated Line | Internet | 5 | 0.1000 | scan, inject, firewall, ls, copy |
| `hw-gpu-1` | Basic Gaming GPU | GPU | 1 | 0.0080 | crack only |
| `hw-gpu-2` | Workstation GPU | GPU | 2 | 0.0250 | crack only |
| `hw-gpu-3` | Compute Cluster GPU | GPU | 3 | 0.0800 | crack only |

---

## Entity 5 — CommandLatencyService

New non-MonoBehaviour class at `Assets/Scripts/Services/CommandLatencyService.cs`.

| Member | Description |
|--------|-------------|
| `CommandLatencyService(Player player)` | Constructor; stores `Player` reference for tier lookups. |
| `float CalculateLatency(CommandLatencyContext context)` | Main entry point; returns effective latency in seconds (already clamped to [minFloor, maxCap]). |
| `private static readonly float[] CpuSpeedupTable` | `[1.0f, 1.5f, 2.5f, 3.5f, 5.0f]` indexed by `tier - 1`. |
| `private static readonly float[] InternetSpeedupTable` | `[1.0f, 1.5f, 2.3f, 3.2f, 4.0f]` indexed by `tier - 1`. |
| `private static readonly float[] GpuSpeedupTable` | `[1.0f, 1.6f, 2.8f, 4.0f]` indexed by `tier`. |
| `private static readonly float[] TargetCpuResistanceTable` | `[1.0f, 1.1f, 1.3f, 1.6f, 2.0f]` indexed by `tier - 1`. |
| `private static readonly float[] TargetBwSpeedupTable` | `[1.0f, 1.2f, 1.5f, 1.8f, 2.2f]` indexed by `tier - 1`. |
| `private static readonly float[] TargetBwResistanceTable` | `[1.0f, 1.05f, 1.12f, 1.20f, 1.30f]` indexed by `tier - 1`. |
| `private static readonly float[] TargetBwScanOverheadTable` | `[1.0f, 1.05f, 1.10f, 1.15f, 1.20f]` indexed by `tier - 1`. |

**CommandLatencyContext** struct at same file location:

```csharp
public readonly struct CommandLatencyContext
{
    public readonly string      CommandVerb;   // "crack", "scan", "inject", etc.
    public readonly SecurityLevel SecurityLevel; // crack only; SecurityLevel.None = N/A
    public readonly Device      TargetDevice;  // null for commands with no specific target
    public readonly long        FileSizeBytes; // copy only; 0 = N/A
}
```

---

## Entity 6 — ICommand.GetLatency (interface extension)

`Assets/Scripts/Interfaces/ICommand.cs` gains a default interface method:

```csharp
float GetLatency(string[] args) => 0f;
```

Instant commands inherit the default. The six latency commands override:

| Command | Override summary |
|---------|-----------------|
| `CrackCommand` | Parses security level from args; builds context with `SecurityLevel`; calls `CommandLatencyService.CalculateLatency`. |
| `ScanCommand` | Detects area vs ip/mac variant from args; for ip/mac looks up target device BW tier; calls service. |
| `InjectCommand` | Looks up target device from LocationService by IP arg; calls service with target. |
| `FirewallCommand` | Looks up target device from LocationService; calls service with target. |
| `LsCommand` | Looks up target device (or uses interactive selection target); calls service with target. |
| `CopyCommand` | Looks up target device and file size; calls service with target and file size. |

All six commands receive `CommandLatencyService` as an additional constructor parameter. `GameManager.Bootstrap()` constructs the service once and passes it when registering commands.
