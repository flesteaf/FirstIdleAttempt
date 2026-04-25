# Research: Location, Network Persistence & Movement

## Decision 1: Root cause of the show networks/ips bug

**Decision**: The bug is in `ShowCommand` and `IncomeService`, not in the save/load pipeline.

**Rationale**: `LocationService.RestoreFromSave()` (line 155) already iterates all `SaveData.Locations` and inserts every location into `_locationCache`. The full dataset is available immediately after load. The bug is that `ShowCommand.networks`, `ShowCommand.ips`, and `IncomeService.OnTick()` all call `GetCurrentLocation()`, which returns only one location. Task A-0 is therefore a **verification** task, not a fix — the fix is exclusively in A-1 through A-4.

**Alternatives considered**: Misread scenario where `RestoreFromSave` only loaded the current location — ruled out by code inspection.

---

## Decision 2: `GetAllKnownLocations()` return type — no heap allocation

**Decision**: Return `_locationCache.Values` directly as `Dictionary<string,Location>.ValueCollection` (implements `IEnumerable<Location>`; exposes `Count`). Do NOT wrap in `ToList()` or `new List<>()`.

**Rationale**: `IncomeService.OnTick()` calls this every second. `Dictionary<,>.ValueCollection` is a struct-backed enumerator — iterating it with `foreach` does allocate a boxed enumerator on older Mono runtimes. To guarantee zero allocation, the implementation should use a `for` loop over a cached `List<Location>` that is updated only when the cache changes (add/remove). This matches the existing `for`-loop pattern in `IncomeService.OnTick()`.

**Implementation note**: Add `private readonly List<Location> _knownLocationsSnapshot = new();` to `LocationService`. Rebuild on cache mutation (add in `GetOrCreateLocation`, remove in `ForgetNetwork`/`ForgetDevice`).

**Alternatives considered**: `IReadOnlyList<Location>` backed by a snapshot — requires a rebuild; chosen approach maintains the snapshot incrementally.

---

## Decision 3: Location name formula collision safety

**Decision**: Use `"node_" + (seed * 77 % 1000)`. Since gcd(77, 1000) = 1, this is a bijection over seeds 0–999 — all names in that range are mathematically unique. Seed 1 → "node_77" (matches user expectation).

**Rationale**: The game will not realistically generate 1000+ locations in a session. If it does, names begin repeating at seed 1000; this is acceptable and can be addressed with a more complex formula in a future milestone.

**Alternatives considered**: UUID-based names (unreadable), sequential numbers (boring), random adjective-noun pairs (requires a word list asset).

---

## Decision 4: Terminal selection mode architecture

**Decision**: Add a `_selectionState` field to `TerminalController` holding a nullable `SelectionState` struct (`string[] Options`, `int HighlightIndex`, `Action<int> OnSelected`). When non-null, `Update()` intercepts Up/Down/Enter and redirects to selection handling instead of command history. Free-text typing calls `OnSelected(-1)` immediately (cancel).

**Rationale**: `TerminalController.Update()` already handles Up/Down for command history via `Keyboard.current`. The same hook point can branch on whether `_selectionState` is set. This avoids adding a new MonoBehaviour or event bus.

**Implementation detail**: `TMP_InputField.onSubmit` fires on Enter — this must be suppressed during selection mode (return early before `ParseCommand`). Arrow keys are consumed in `Update()` before TMP processes them (wasPressedThisFrame guard).

**Alternatives considered**: A separate `SelectionController` MonoBehaviour — unnecessary indirection for a feature of this scope.

---

## Decision 5: `TargetedDevice`/`TargetedNetwork` removal strategy

**Decision**: Delete both `[System.NonSerialized]` fields from `Player.cs`. Fix compile errors command-by-command. `ScanCommand`'s `scan ip` and `scan mac` handlers already populate these fields — remove those assignments too (Task I-3). Commands that read them (`FirewallCommand`, `LsCommand`, `CopyCommand`) are refactored in Task Group J.

**Rationale**: The fields are `[System.NonSerialized]` so they carry no save-data risk. Deletion is safe and clean. No migration needed.

---

## Decision 6: `DeviceSelector` helper placement

**Decision**: Create `Assets/Scripts/Services/Commands/DeviceSelector.cs` as a static class with a single method `AwaitDevice(TerminalController, LocationService, Action<Device>)`. Placed in `Commands/` because it is a command-layer concern, not a service concern.

**Rationale**: Used only by `FirewallCommand`, `LsCommand`, `CopyCommand`, and the network-selection step of `InjectCommand`. Keeps `LocationService` free of UI coupling.

---

## Decision 7: HUD location name display

**Decision**: Add a new `[SerializeField] private TextMeshProUGUI _locationLabel` field to `HUDController`. Update it in `Refresh()` alongside the BTC label. The `MoveCommand` does not call `HUDController` directly — `HUDController.OnTick()` reads `LocationService.GetCurrentLocationName()` each tick and updates only when the name changes (same caching pattern as `_lastBtc`).

**Rationale**: Keeps coupling unidirectional (HUD reads from service, not commanded by `MoveCommand`). The tick rate (1 s) means the label updates within one second of movement — imperceptible latency.

**Prefab impact**: `HUDController` component in `Terminal.prefab` (or the HUD prefab) needs a new `TextMeshProUGUI` child object wired to `_locationLabel`. This is a prefab-side change tracked in the PR.

---

## Decision 8: Save schema migration (v1 → v2)

**Decision**: Migration runs in `GameManager.Bootstrap()` after `SaveSystem.Load()`. If `SaveData.Version < 2`: iterate all `LocationSaveData`, set `Name` using `"node_" + (int.Parse(s.Id) * 77 % 1000)`, set `CurrentLocationName` to the first entry's name (or "node_77" if list empty). Then set `Version = 2` and save immediately.

**Rationale**: Inline migration at bootstrap is the simplest path — no separate migration class needed at this scale. Version 2 is written back so migration only runs once.

---

## Decision 9: `ForgetResult` type placement

**Decision**: `Assets/Scripts/Models/ForgetResult.cs` — `readonly struct` with `bool Success`, `string Message`, `int IpsRemoved`, and a static factory `Ambiguous(string[] locationNames)` that returns `Success=false` with a formatted message.

**Rationale**: Model layer, not service layer — it is a data transfer type, not a service. Consistent with `CommandResult` placement in `Interfaces/`.

---

## Decision 10: `show locations` empty-state and `show networks`/`show ips` column headers

**Decision**: All three show sub-commands always render a fixed column header row before any data rows. Headers: `show networks` → `NETWORK  LOCATION  SECURITY  STATUS`; `show ips` → `IP  NETWORK  LOCATION  TYPE  INCOME/s`; `show locations` → `LOCATION  NETWORKS  INFECTED`.

**Rationale**: Consistent with Q3 clarification answer (Option C). Gives players immediate context about what the output represents even when the game is new.
