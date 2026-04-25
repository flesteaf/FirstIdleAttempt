# Command Schema: Location, Network Persistence & Movement

All commands follow the `ICommand` contract: `Execute(string[] args): CommandResult` where `CommandResult` has `bool Success` and `string Message` only. Interactive selection (arrow keys, numbered list) is mediated by `TerminalController.AwaitSelection()` and does not produce a `CommandResult` until the player confirms or cancels.

---

## New Commands

### `move`

**Verb**: `move`  
**Registered in**: `GameManager.RegisterCommands()`

| Syntax | Args | Output | Side-effects |
|--------|------|--------|--------------|
| `move` | none | `"Moved to <name>."` | Generates new location; updates `_currentLocationId`; HUD location label refreshes within next tick |
| `move <name>` | `name`: known location name | `"Moved to <name>."` on success; `"Already at <name>."` if already there; `"Location '<name>' not found. Use 'move' without arguments to discover a new location."` on unknown | Updates `_currentLocationId` on success; no state change on failure |

**Error cases**:
- Unknown location name → `Success = false`
- No change (already at location) → `Success = true`, informational message

---

### `forget`

**Verb**: `forget`  
**Registered in**: `GameManager.RegisterCommands()`

| Syntax | Args | Output | Side-effects |
|--------|------|--------|--------------|
| `forget network <SSID>` | `SSID`: network name | Success: `"Forgot network '<SSID>' and removed N infected device(s)."` | Removes network + all devices from `_locationCache`; stops income contributions |
| `forget network <SSID>` (ambiguous) | SSID exists at 2+ locations | Interactive numbered list of location names rendered; awaits selection; on confirm → targeted removal; on cancel → no change | Enters selection mode |
| `forget network <SSID> at <location>` | `SSID` + `location` name | Same success message as above, scoped to that location | Bypasses interactive step |
| `forget ip <IP>` | `IP`: device IP address | `"Forgot device <IP>."` | Removes device from its network; stops income if infected |

**Error cases**:
- `forget network <unknown-SSID>` → `"Network '<SSID>' not found in any known location."`
- `forget ip <unknown-IP>` → `"IP not found in any known location."`
- Invalid syntax → usage error listing all three forms
- `forget network <SSID> at <unknown-location>` → `"Location '<location>' not found."`

---

## Modified Commands

### `scan` (behaviour changes)

**Changes**: Remove double-scan movement. Add no-location error. Remove `TargetedDevice`/`TargetedNetwork` assignment.

| Syntax | Before | After |
|--------|--------|-------|
| `scan` (first call) | Lists networks at current location; sets `IsVisited = true` | Lists networks at current location (no state change to IsVisited) |
| `scan` (second call) | Called `MoveToNextLocation()` and listed new networks | Same as first call — lists current location networks only |
| `scan` (no location) | Not handled | Returns `"No location available. Use 'move' to discover a location first."` |
| `scan ip <IP>` | Lists device details; sets `Player.TargetedDevice` | Lists device details only; no targeting assignment |
| `scan mac <MAC>` | Lists device details; sets `Player.TargetedNetwork` | Lists device details only; no targeting assignment |

---

### `inject` (interactive selection)

**Changes**: When type or network is missing, enter interactive selection mode.

| Syntax | Behaviour |
|--------|-----------|
| `inject <type> <SSID>` | Unchanged — direct injection |
| `inject <type>` | `type` resolved; shows numbered list of accessible networks at current location; Enter confirms; Cancel or free text aborts |
| `inject` | Shows numbered list of unlocked malware types first; on confirm → shows network list (as above) |

**Selection lists**:
- Type selection: `["miner", …unlocked types…]` + implicit "Cancel" last
- Network selection: `[SSID list from current location]` + implicit "Cancel" last
- Empty unlocked types → error (should not occur; Miner always available)
- Empty accessible networks → error `"No accessible networks at current location."` (no selection prompt)

---

### `firewall` (optional IP; interactive fallback)

**New syntax**: `firewall <disable|enable> [<IP>]`

| Syntax | Behaviour |
|--------|-----------|
| `firewall disable <IP>` | Disables firewall on device at `IP` directly |
| `firewall enable <IP>` | Enables firewall on device at `IP` directly |
| `firewall disable` (no IP) | Shows numbered list of discovered devices at current location; on selection → disables firewall on chosen device |
| `firewall enable` (no IP) | Same as above with enable |

**Error cases**:
- Unknown IP → `"Device '<IP>' not found."`
- No discovered devices at current location → `"No devices discovered at current location. Use 'scan' first."`
- Invalid sub-command → usage error

---

### `ls` (optional IP; interactive fallback)

**New syntax**: `ls [<IP>]`

| Syntax | Behaviour |
|--------|-----------|
| `ls <IP>` | Lists files on device at `IP` directly |
| `ls` (no IP) | Shows numbered list of discovered devices at current location; on selection → lists files on chosen device |

**Error cases**:
- Unknown IP → `"Device '<IP>' not found."`
- No discovered devices → `"No devices discovered at current location. Use 'scan' first."`

---

### `copy` (optional IP; interactive fallback)

**New syntax**: `copy <filename> [<IP>]`

| Syntax | Behaviour |
|--------|-----------|
| `copy <filename> <IP>` | Copies named file from device at `IP` directly |
| `copy <filename>` (no IP) | Shows numbered list of discovered devices at current location; on selection → copies file from chosen device |

**Error cases**:
- Unknown IP → `"Device '<IP>' not found."`
- File not found on device → `"File '<filename>' not found on device."`
- No discovered devices → `"No devices discovered at current location. Use 'scan' first."`

---

### `show` (new sub-command + column headers)

**New syntax**: `show locations`

| Syntax | Output format |
|--------|---------------|
| `show networks` | Column header always: `NETWORK  LOCATION  SECURITY  STATUS`; then one row per discovered network across all known locations |
| `show ips` | Column header always: `IP  NETWORK  LOCATION  TYPE  INCOME/s`; then one row per infected device across all known locations |
| `show locations` | Column header always: `LOCATION  NETWORKS  INFECTED`; then one row per known location; current location prefixed with `>` |

All three show the column header row even when there is no data to display.

---

## Interactive Selection Protocol

When a command enters selection mode via `TerminalController.AwaitSelection()`:

1. Options are rendered as a numbered list. "Cancel" is always the last entry (appended automatically — callers do not include it).
2. Up/Down arrow keys move the `>` highlight.
3. Typing a digit (1…N) moves highlight to that row without confirming.
4. Enter confirms the highlighted option: "Cancel" row → `onSelected(-1)`; other → `onSelected(index)`.
5. Typing any non-digit printable character, or a digit out of range → `onSelected(-1)` immediately, input discarded.
6. While in selection mode, free-text command submission is suppressed.

**Example rendering** for `inject` with miner unlocked:

```
Select malware type:
> 1. miner
  2. Cancel
```

After Down arrow:

```
Select malware type:
  1. miner
> 2. Cancel
```

---

## Help Entries

| Command | Usage string |
|---------|-------------|
| `move` | `move — discover a new location; move <name> — travel to a known location` |
| `forget` | `forget network <SSID> [at <location>] — remove network and its IPs; forget ip <IP> — remove a single infected device` |
| `inject` | `inject — guided type and network selection; inject <type> — select network; inject <type> <SSID> — inject directly` |
| `firewall` | `firewall <disable\|enable> [<IP>] — toggle device firewall; omit IP to select interactively` |
| `ls` | `ls [<IP>] — list files on a device; omit IP to select interactively` |
| `copy` | `copy <filename> [<IP>] — copy file from device; omit IP to select interactively` |
| `show` | `show networks\|ips\|locations — list discovered networks, infected IPs, or all known locations` |
