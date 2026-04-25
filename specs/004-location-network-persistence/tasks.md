# Tasks: Location, Network Persistence & Movement

**Input**: Design documents from `specs/004-location-network-persistence/`  
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ ✓

**Format**: `[ID] [P?] [Story?] Description with file path`  
- **[P]**: Parallelisable (different files, no dependency on in-flight tasks)  
- **[USN]**: Maps to User Story N from spec.md

---

## Phase 1: Setup

No setup required — project already initialised with Unity 6 + NUnit + TextMeshPro.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Model and field changes that unblock all user stories. No user story work begins until this phase is complete.

**⚠️ CRITICAL**: Complete before any Phase 3+ task starts.

- [x] T001 [P] Add `public string Name` field to `Location` class in `Assets/Scripts/Models/Location.cs`
- [x] T002 [P] Add `public string Name` field to `LocationSaveData` class in `Assets/Scripts/Models/SaveData.cs`
- [x] T003 [P] Add `public string CurrentLocationName` field to `SaveData` class in `Assets/Scripts/Models/SaveData.cs`
- [x] T004 [P] Create `ForgetResult` readonly struct with `bool Success`, `string Message`, `int IpsRemoved`, and static factory `ForgetResult.Ambiguous(string[] locationNames)` in `Assets/Scripts/Models/ForgetResult.cs`
- [x] T005 Delete `[System.NonSerialized] public Device TargetedDevice` and `[System.NonSerialized] public Network TargetedNetwork` fields from `Assets/Scripts/Models/Player.cs`; fix all resulting compile errors in `FirewallCommand.cs`, `LsCommand.cs`, `CopyCommand.cs`, `ScanCommand.cs`, and any other files that referenced them

**Checkpoint**: Project compiles with zero errors. No test references `Player.TargetedDevice` or `Player.TargetedNetwork`.

---

## Phase 3: User Story 1 — Reload Preserves All Discovered Networks and Infected IPs (Priority: P1) 🎯 MVP

**Goal**: Fix `show networks`, `show ips`, and `IncomeService` to aggregate across all known locations so data visible before a save is also visible after reload.

**Independent Test**: Start fresh session, scan networks, infect IPs, quit, relaunch — `show networks` and `show ips` display all previously discovered data. HUD income matches pre-close value within one tick.

- [x] T006 [P] [US1] Verify `LocationService.RestoreFromSave()` (around line 155) iterates all `SaveData.Locations` entries and inserts each into `_locationCache`; add unit test in `Assets/Tests/EditMode/LocationServiceTests.cs`: build `SaveData` with 3 `LocationSaveData` entries, call `RestoreFromSave()`, assert `_locationCache.Count == 3`
- [x] T007 [P] [US1] Generate location name deterministically in `LocationService.GenerateLocation(seed)` in `Assets/Scripts/Services/LocationService.cs`: `location.Name = "node_" + (seed * 77 % 1000);` using named constants (`private const int NameMultiplier = 77; private const int NameModulus = 1000;`); add unit test asserting seed 1 → "node_77" and all seeds 1–999 produce distinct names
- [x] T008 [US1] Add `private readonly List<Location> _knownLocationsSnapshot = new();` to `LocationService` and expose `IReadOnlyCollection<Location> GetAllKnownLocations()` returning `_locationCache.Values`; rebuild `_knownLocationsSnapshot` on every cache mutation (add in `GetOrCreateLocation`, remove in `ForgetNetwork`/`ForgetDevice`); add unit test in `Assets/Tests/EditMode/LocationServiceTests.cs`: restore from 3-location save → `GetAllKnownLocations()` returns 3; verify same reference returned on second call (no allocation) — in `Assets/Scripts/Services/LocationService.cs`
- [x] T009 [US1] Map `LocationSaveData.Name` → `Location.Name` in `LocationService.FromSaveData()`; back-fill missing name using `"node_" + (int.Parse(locationSaveData.Id) * 77 % 1000)` for v1 saves (Name null or empty); add unit tests: v1 save (Name=null, Id="1") → "node_77"; Id="2" → "node_154" — in `Assets/Scripts/Services/LocationService.cs`
- [x] T010 [P] [US1] Add `public string CurrentLocationName` to `SaveData` and add unit test confirming `JsonUtility` round-trip preserves it — in `Assets/Scripts/Models/SaveData.cs` (covers T003 integration)
- [x] T011 [US1] Add v1→v2 migration in `GameManager.Bootstrap()` (or `SaveSystem.cs`): if `SaveData.Version < 2`, back-fill `LocationSaveData.Name` from seed for each entry, set `SaveData.CurrentLocationName` to first location's name (or "node_77" if empty), set `Version = 2`, re-save; add unit test with synthetic v1 fixture — no crash, all names populated, `CurrentLocationName` set, version is 2 — in `Assets/Scripts/Core/GameManager.cs`
- [x] T012 [US1] Update `LocationService.ToSaveData()` to write `saveData.CurrentLocationName = GetCurrentLocationName()` and update `FromSaveData()` to call `SetCurrentLocation(saveData.CurrentLocationName)` after all locations are loaded (falling back to first location if null); add unit test: save → reload → `GetCurrentLocationName()` is same — in `Assets/Scripts/Services/LocationService.cs`
- [x] T013 [US1] Fix `ShowCommand` `networks` handler: replace `GetCurrentLocation()` with `GetAllKnownLocations()`; group output by location name; always render column header `NETWORK  LOCATION  SECURITY  STATUS` first even when empty; add unit tests in `Assets/Tests/EditMode/ShowCommandTests.cs`: 2 locations × 2 networks → header + 4 lines with 2 location sub-headers; 0 known locations → header only — in `Assets/Scripts/Services/Commands/ShowCommand.cs`
- [x] T014 [US1] Fix `ShowCommand` `ips` handler: iterate all devices across `GetAllKnownLocations()` filtering for `ActiveMalware != null`; always render column header `IP  NETWORK  LOCATION  TYPE  INCOME/s` first even when empty; add unit tests: infected device in A + infected device in B → both listed; no infected devices → header only — in `Assets/Scripts/Services/Commands/ShowCommand.cs`
- [x] T015 [US1] Fix `IncomeService.OnTick()`: replace `GetCurrentLocation()` with `GetAllKnownLocations()` in the tick loop (no allocation — `ValueCollection` used directly); add unit test in `Assets/Tests/EditMode/IncomeServiceTests.cs`: miner in location A + spammer in location B → combined income per tick equals sum of both rates — in `Assets/Scripts/Services/IncomeService.cs`
- [ ] T016 [US1] Profile `IncomeService.OnTick()` with 10+ known locations each containing 4 devices in Unity Profiler; confirm < 1 ms per call with zero allocations; attach Profiler screenshot to PR (Constitution §IV gate) — in `Assets/Scripts/Services/IncomeService.cs` (validation, no code change)

**Checkpoint**: After T006–T016, a save/reload cycle shows all networks and IPs; income matches pre-close value within one tick. US1 fully functional.

---

## Phase 4: User Story 2 — Player Can Move Between Locations Explicitly (Priority: P2)

**Goal**: `move` command is the sole location navigation mechanism; `scan` never changes location; HUD displays current location name.

**Independent Test**: Run `move` (no args) → new location name printed + HUD updates. Run `move node_77` → returns to starting location + HUD updates. Run `move unknown` → error, HUD unchanged. `scan` twice in sequence → location unchanged.

- [x] T017 [P] [US2] Add `bool SetCurrentLocation(string name)` to `LocationService`: search `_locationCache` for location with matching `Name`, update `_currentLocationId`, return false if not found; add unit tests in `Assets/Tests/EditMode/LocationServiceTests.cs`: known name → true + location changes; unknown name → false + unchanged — in `Assets/Scripts/Services/LocationService.cs`
- [x] T018 [P] [US2] Add `string GetCurrentLocationName()` to `LocationService`: return `GetCurrentLocation().Name`; add unit test after `SetCurrentLocation("node_77")` → `GetCurrentLocationName()` returns "node_77" — in `Assets/Scripts/Services/LocationService.cs`
- [x] T019 [US2] Implement `MoveCommand` in `Assets/Scripts/Services/Commands/MoveCommand.cs` (new file): `move` (no args) → `MoveToNextLocation()`, return `CommandResult.Ok("Moved to <name>.")`; `move <name>` → `SetCurrentLocation(name)`: success → `Ok("Moved to <name>.")`, already there → `Ok("Already at <name>.")`, unknown → `Fail("Location '<name>' not found. Use 'move' without arguments to discover a new location.")`; add unit tests in `Assets/Tests/EditMode/MoveCommandTests.cs`: no-args success, named-location success, unknown-name failure, same-location already-there message
- [x] T020 [US2] Register `MoveCommand` in `GameManager.RegisterCommands()`: `parser.Register("move", new MoveCommand(_locationService));` — in `Assets/Scripts/Core/GameManager.cs`
- [x] T021 [US2] Remove double-scan movement from `ScanCommand`: delete the logic that calls `MoveToNextLocation()` on a second consecutive `scan` invocation and any `IsVisited`-based state tracking for movement; add unit test in `Assets/Tests/EditMode/ScanCommandTests.cs`: calling `Execute()` twice in sequence does NOT change `GetCurrentLocationName()` — in `Assets/Scripts/Services/Commands/ScanCommand.cs`
- [x] T022 [US2] Add no-location error to `ScanCommand`: at start of no-args `scan` handler, if `_locationCache` is empty return `CommandResult.Fail("No location available. Use 'move' to discover a location first.")`; add unit tests: empty cache → failure result containing "move"; populated cache → normal scan output — in `Assets/Scripts/Services/Commands/ScanCommand.cs`
- [x] T023 [US2] Add `[SerializeField] private TextMeshProUGUI _locationLabel` to `HUDController`; in `OnTick()` read `_locationService.GetCurrentLocationName()` and update `_locationLabel.text` only when the name changes (cache pattern matching existing `_lastBtc`); add `_locationLabel` child `TextMeshProUGUI` object to Terminal prefab and wire to the field — in `Assets/Scripts/UI/HUDController.cs` and `Assets/Prefabs/Terminal.prefab`

**Checkpoint**: `move` is fully functional. `scan` never changes location. HUD shows current location name updated within one tick after `move`.

---

## Phase 5: User Story 4 — `inject` Guides Player Through Interactive Type and Network Selection (Priority: P2)

**Goal**: `inject` with no/partial args presents arrow-navigable numbered lists; full-arg `inject <type> <SSID>` unchanged.

**Independent Test**: Run `inject` with no args → numbered malware type list with "Cancel" last; navigate with arrows; Enter → numbered network list; select → injection proceeds as if `inject <type> <SSID>` had been typed.

- [x] T024 [US4] Add `void AwaitSelection(string[] options, Action<int> onSelected)` to `TerminalController` in `Assets/Scripts/UI/TerminalController.cs`: adds nullable `_selectionState` struct (`string[] Options`, `int HighlightIndex`, `Action<int> OnSelected`); "Cancel" always appended as last entry; `Update()` intercepts Up/Down (move highlight) and Enter (confirm: Cancel row → `onSelected(-1)`, other → `onSelected(index)`); typing a digit (1…N) moves highlight without confirming; typing any other printable character or out-of-range digit → `onSelected(-1)` immediately and discards input; free-text submission (onSubmit) suppressed while selection active; add unit tests in `Assets/Tests/EditMode/TerminalControllerTests.cs`: list rendering, arrow navigation, Enter confirm, digit jump, free-text cancel, out-of-range no-crash
- [x] T025 [P] [US4] Extend `InjectCommand` with interactive type selection (no-args path): when `inject` is called with no type argument, call `TerminalController.AwaitSelection` with player's unlocked malware types; `onSelected(-1)` → abort; valid selection → resolve type and proceed to network step; add unit tests in `Assets/Tests/EditMode/InjectCommandTests.cs`: one type unlocked → list shows 1 + Cancel; cancel → abort — in `Assets/Scripts/Services/Commands/InjectCommand.cs`
- [x] T026 [US4] Extend `InjectCommand` with interactive network selection (after type resolved, no network arg): call `TerminalController.AwaitSelection` with accessible network SSIDs at current location; `onSelected(-1)` → abort; valid selection → proceed with injection preconditions; no accessible networks → error `"No accessible networks at current location. Crack a network first."` without prompting; add unit tests: 2 accessible networks → list shown; cancel → abort; 0 networks → error returned — in `Assets/Scripts/Services/Commands/InjectCommand.cs`
- [x] T027 [P] [US4] Verify `inject <type> <SSID>` (full-args path) is unchanged: run all existing `InjectCommand` unit tests and confirm all pass without modification — in `Assets/Scripts/Services/Commands/InjectCommand.cs`
- [x] T028 [P] [US4] Remove `TargetedDevice`/`TargetedNetwork` assignment lines from `ScanCommand`'s `scan ip` and `scan mac` handlers; confirm `scan ip <IP>` still outputs device details; add unit test: `scan ip 192.168.1.10` executes successfully, outputs device details, no assignment to removed fields — in `Assets/Scripts/Services/Commands/ScanCommand.cs`

**Checkpoint**: `inject` interactive flow works end-to-end. Full-arg path unaffected. Arrow navigation, Cancel, and free-text cancel all function correctly.

---

## Phase 6: User Story 6 — `firewall`, `ls`, and `copy` Accept Explicit IP or Interactive Device Selection (Priority: P2)

**Goal**: Three commands work without implicit targeting — explicit IP arg or interactive device list.

**Independent Test**: Run `firewall disable` with no IP → device list shown; select a device → firewall toggled exactly as if `firewall disable <IP>` had been typed. Repeat pattern for `ls` and `copy`.

- [x] T029 [US6] Implement `DeviceSelector` static helper in `Assets/Scripts/Services/Commands/DeviceSelector.cs` (new file): `static void AwaitDevice(TerminalController terminal, LocationService locationService, Action<Device> onSelected)` — builds list of scanned devices at current location, calls `terminal.AwaitSelection`, maps index back to `Device`; 0 scanned devices → returns error without calling `AwaitSelection`; add unit tests in `Assets/Tests/EditMode/DeviceSelectorTests.cs`: 2 scanned devices → `AwaitSelection` called with 2 entries; 0 devices → error path, no `AwaitSelection` call
- [x] T030 [P] [US6] Update `FirewallCommand` for new syntax `firewall <disable|enable> [<IP>]` in `Assets/Scripts/Services/Commands/FirewallCommand.cs`: if IP provided → resolve device directly from `LocationService` and apply; if IP omitted → call `DeviceSelector.AwaitDevice`; cancel → abort; no devices → error; add unit tests in `Assets/Tests/EditMode/FirewallCommandTests.cs`: explicit-IP path disables directly; no-IP path shows list; cancel → no change; no devices → error
- [x] T031 [P] [US6] Update `LsCommand` for new syntax `ls [<IP>]` in `Assets/Scripts/Services/Commands/LsCommand.cs`: if IP provided → list files directly; if omitted → call `DeviceSelector.AwaitDevice`; add unit tests in `Assets/Tests/EditMode/LsCommandTests.cs`: explicit-IP path lists directly; no-IP path shows list; no devices → error
- [x] T032 [P] [US6] Update `CopyCommand` for new syntax `copy <filename> [<IP>]` in `Assets/Scripts/Services/Commands/CopyCommand.cs`: if IP provided → copy directly; if omitted → call `DeviceSelector.AwaitDevice`; file not found on selected device → error; add unit tests in `Assets/Tests/EditMode/CopyCommandTests.cs`: explicit-IP + found file → copies; no-IP → list shown; file not found → error; no devices → error

**Checkpoint**: `firewall`, `ls`, `copy` all work with explicit IP and with interactive selection. No implicit targeting used anywhere.

---

## Phase 7: User Story 3 — Player Can Forget Networks or IPs (Priority: P3)

**Goal**: `forget network <SSID>` and `forget ip <IP>` remove entries and stop income; SSID ambiguity shows a selection list.

**Independent Test**: Infect a device → confirm income in `show ips` → `forget ip <IP>` → `show ips` no longer lists it → income drops within one tick.

- [x] T033 [P] [US3] Add `ForgetNetwork(string ssid, string locationName = null): ForgetResult` to `LocationService` in `Assets/Scripts/Services/LocationService.cs`: find all networks matching SSID; if `locationName` provided filter to that location; multiple unfiltered matches → return `ForgetResult.Ambiguous(locationNames)`; single match → remove network + all devices, rebuild `_knownLocationsSnapshot`, return success with `IpsRemoved` count; add unit tests in `Assets/Tests/EditMode/LocationServiceTests.cs`: 3 infected devices removed → `IpsRemoved == 3`; ambiguous → `Success == false` with 2 location names; location filter narrows correctly; forgotten device not present in save → `OfflineIncomeCalculator` does not credit it on reload
- [x] T034 [P] [US3] Add `ForgetDevice(string ip): ForgetResult` to `LocationService` in `Assets/Scripts/Services/LocationService.cs`: search all locations for device with matching IP; remove from parent network's device list; rebuild `_knownLocationsSnapshot`; return `ForgetResult`; add unit tests: known IP → removed, `IpsRemoved == 1`; unknown IP → `Success == false`
- [x] T035 [US3] Implement `ForgetCommand` in `Assets/Scripts/Services/Commands/ForgetCommand.cs` (new file): `forget network <SSID>` → `ForgetNetwork(ssid, null)`; if `Ambiguous` → render numbered list via `TerminalController.AwaitSelection`, on confirm call `ForgetNetwork(ssid, locationName)`, on cancel → abort; `forget network <SSID> at <location>` → `ForgetNetwork(ssid, location)` directly; `forget ip <IP>` → `ForgetDevice(ip)`; invalid syntax → usage error listing all three forms; add unit tests in `Assets/Tests/EditMode/ForgetCommandTests.cs`: single SSID → removed; ambiguous SSID → list shown, valid number → targeted removal, invalid number → re-displayed; `at <location>` bypass; `forget ip` → correct device removed; invalid syntax → usage error
- [x] T036 [US3] Register `ForgetCommand` in `GameManager.RegisterCommands()`: `parser.Register("forget", new ForgetCommand(_locationService, _terminalController));` — in `Assets/Scripts/Core/GameManager.cs`

**Checkpoint**: `forget network` and `forget ip` fully functional. SSID ambiguity resolved via selection list. Income drops within one tick of forget.

---

## Phase 8: User Story 5 — Player Can View All Known Locations (Priority: P3)

**Goal**: `show locations` lists every known location with name, network count, and infected IP count; current location marked with `>`.

**Independent Test**: Visit 3 locations, run `show locations` — all 3 appear with accurate counts; current location has `>` prefix. Fresh game → column header only.

- [x] T037 [US5] Add `"locations"` handler to `ShowCommand` in `Assets/Scripts/Services/Commands/ShowCommand.cs`: always render column header `LOCATION  NETWORKS  INFECTED`; iterate `GetAllKnownLocations()`; format each line as `[>] <name>  <networkCount> networks  <infectedCount> infected`; mark current with `>`; empty list → header only; add unit tests in `Assets/Tests/EditMode/ShowCommandTests.cs`: 3 known locations, player at location 2 → header + 3 lines, line 2 has `>`, counts accurate; 0 locations → header only

**Checkpoint**: `show locations` lists all visited locations with accurate data and `>` marker after reload.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Help text, XML docs, UX gate, and profiler validation across all user stories.

- [x] T038 [P] Add `move` and `forget` usage strings to `HelpCommand` in `Assets/Scripts/Services/Commands/HelpCommand.cs`: `move` → "move — discover a new location; move \<name\> — travel to a known location"; `forget` → "forget network \<SSID\> [at \<location\>] — remove network and its IPs; forget ip \<IP\> — remove a single infected device"; add tests: `help move` and `help forget` return non-empty usage lines
- [x] T039 [P] Append `show locations` to `show` usage in `HelpCommand` in `Assets/Scripts/Services/Commands/HelpCommand.cs`; add test: `help show` output contains "locations"
- [x] T040 [P] Update `inject` usage in `HelpCommand` in `Assets/Scripts/Services/Commands/HelpCommand.cs`: "inject — guided type and network selection; inject \<type\> — select network; inject \<type\> \<SSID\> — inject directly"; add test: `help inject` contains all three forms
- [x] T041 [P] Update `firewall`, `ls`, `copy` usage in `HelpCommand` in `Assets/Scripts/Services/Commands/HelpCommand.cs`: `firewall` → "firewall \<disable|enable\> [\<IP\>] — toggle device firewall; omit IP to select interactively"; `ls` → "ls [\<IP\>] — list files on a device; omit IP to select interactively"; `copy` → "copy \<filename\> [\<IP\>] — copy file from device; omit IP to select interactively"; add tests: each `help` output contains "interactively"
- [x] T042 [P] Add XML `/// <summary>` documentation to all new public `LocationService` methods: `GetAllKnownLocations()`, `SetCurrentLocation()`, `GetCurrentLocationName()`, `ForgetNetwork()`, `ForgetDevice()`; include `/// <param>` for non-obvious parameters (e.g., `locationName` in `ForgetNetwork`) — in `Assets/Scripts/Services/LocationService.cs`
- [ ] T043 Run `move`, `move <name>`, `move <unknown>`, `forget network <SSID>`, `forget ip <IP>`, and `forget network <ambiguous-SSID>` in the Unity game terminal; confirm output format (line prefix style, capitalisation, error wording, spacing) is visually consistent with existing commands (`scan`, `crack`, `inject`); document ≥6 terminal output samples side-by-side in PR description (Constitution §III UX gate) — manual validation, no code change

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Skipped — project already initialised
- **Foundational (Phase 2)**: No dependencies — start immediately. BLOCKS all user story phases
- **US1 (Phase 3)**: Depends on Phase 2 (T001–T005). T006–T007 parallelisable; T008 depends on T006; T009–T012 depend on T001–T003; T013–T015 depend on T008
- **US2 (Phase 4)**: Depends on Phase 2 + US1 (T008, T009). T017–T018 parallelisable; T019 depends on T017–T018; T021–T022 independent of T019; T023 depends on T018
- **US4 (Phase 5)**: Depends on Phase 2 (T005) and Phase 4 (T019 for `MoveCommand` pattern). T024 first; T025–T028 can run in parallel after T024
- **US6 (Phase 6)**: Depends on Phase 2 (T005) and Phase 5 (T024 — `AwaitSelection` must exist). T029 first; T030–T032 parallelisable after T029
- **US3 (Phase 7)**: Depends on Phase 2 (T004 — `ForgetResult`) and Phase 3 (T008 — `GetAllKnownLocations`). T033–T034 parallelisable; T035 depends on both and on T024 (selection mode from Phase 5)
- **US5 (Phase 8)**: Depends on Phase 3 (T008). Independent of US3 and can run in parallel with Phase 7
- **Polish (Phase 9)**: Depends on all user story phases complete. T038–T042 all parallelisable

### Parallel Opportunities

Within Phase 2: T001–T004 run in parallel; T005 runs after T001 is merged (needs `Device` reference removed)  
Within Phase 3: T006–T007 in parallel; T013–T014 in parallel after T008  
Within Phase 6: T030–T032 in parallel after T029  
Within Phase 9: T038–T042 all in parallel

---

## Parallel Example: User Story 1 (Phase 3)

```
# Start in parallel:
Task T006: Verify FromSaveData + unit test (LocationServiceTests.cs)
Task T007: Name formula in GenerateLocation (LocationService.cs)

# After T006:
Task T008: GetAllKnownLocations + _knownLocationsSnapshot (LocationService.cs)
Task T010: CurrentLocationName round-trip test (SaveData.cs)

# After T008:
Task T009: Name restore + migration in FromSaveData (LocationService.cs)
Task T013: Fix show networks (ShowCommand.cs)  ← parallel
Task T014: Fix show ips (ShowCommand.cs)        ← same file, do sequentially
Task T015: Fix IncomeService.OnTick (IncomeService.cs) ← parallel

# After T009–T010:
Task T011: v1→v2 migration in GameManager (GameManager.cs)
Task T012: Write/read CurrentLocationName in LocationService (LocationService.cs)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Foundational (T001–T005)
2. Complete Phase 3: User Story 1 (T006–T016)
3. **STOP and VALIDATE**: `show networks`, `show ips`, income all work after reload
4. Deploy/demo MVP — core bug fixed

### Incremental Delivery

1. Phase 2 → Phase 3 (US1) → Demo: persistence bug fixed ✓  
2. + Phase 4 (US2) → Demo: explicit move command, HUD location label ✓  
3. + Phase 5 (US4) → Demo: inject interactive selection ✓  
4. + Phase 6 (US6) → Demo: firewall/ls/copy no implicit targeting ✓  
5. + Phase 7 (US3) → Demo: forget network/ip ✓  
6. + Phase 8 (US5) → Demo: show locations ✓  
7. Phase 9: Polish across all stories

---

## Notes

- `[P]` tasks touch different files or have no dependency on in-flight tasks in the same phase
- `[USN]` label maps each task to its user story for traceability
- Each story phase produces a fully functional, independently testable increment
- Tests follow Red-Green-Refactor (Constitution §II): write test → confirm it fails → implement → confirm it passes
- Commit after each task or logical group (Constitution §II)
- Constitution §IV: no hot-path allocations; `GetAllKnownLocations()` returns `ValueCollection`, not a new `List`
- Constitution §I: named constants for `77` and `1000` in name formula; XML docs on all new `LocationService` public methods
- Manual UX gate (T043) must be completed and documented in PR before merge
