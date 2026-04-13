---
description: "Task list for Hacking Idle Game — Core Game Loop"
---

# Tasks: Hacking Idle Game — Core Game Loop

**Input**: Design documents from `specs/001-hacking-idle-core/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅ quickstart.md ✅

**Tests**: Included — Constitution Principle II mandates TDD (Red-Green-Refactor).
Tests MUST be written and confirmed failing before each implementation task.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story (US1, US2, US3)
- Paths are relative to repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project scaffolding, assembly definitions, shared interfaces and enums.
No game logic here — only the skeleton that every other phase depends on.

- [X] T001 Create game script folder structure: `Assets/Scripts/Core/`, `Assets/Scripts/Models/`, `Assets/Scripts/Services/`, `Assets/Scripts/Services/Commands/`, `Assets/Scripts/Data/`, `Assets/Scripts/UI/`, `Assets/Scripts/Interfaces/`
- [X] T002 Create test folder structure: `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`
- [X] T003 Create main Assembly Definition at `Assets/Scripts/HackYourWay.asmdef` — set `autoReferenced: true`; no test references
- [X] T004 Create edit-mode test Assembly Definition at `Assets/Tests/EditMode/HackYourWay.EditMode.Tests.asmdef` — set `autoReferenced: false`, `overrideReferences: true`, `precompiledReferences: ["nunit.framework.dll"]`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`; references: HackYourWay, UnityEngine.TestRunner, UnityEditor.TestRunner
- [X] T005 Create play-mode test Assembly Definition at `Assets/Tests/PlayMode/HackYourWay.PlayMode.Tests.asmdef` — set `autoReferenced: false`, `overrideReferences: true`, `precompiledReferences: ["nunit.framework.dll"]`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`, `includePlatforms: []`; references: HackYourWay, UnityEngine.TestRunner
- [X] T006 [P] Create `ICommand` interface in `Assets/Scripts/Interfaces/ICommand.cs` — method `Execute(string[] args)` returning `CommandResult` (value type: bool success + string message)
- [X] T007 [P] Create `ITickable` interface in `Assets/Scripts/Interfaces/ITickable.cs` — method `OnTick(double deltaSeconds)`
- [X] T008 [P] Create all enums in individual files under `Assets/Scripts/Models/`: `CurrencyType.cs` (Bitcoin), `SecurityLevel.cs` (None/WEP/WPA/WPA2), `MalwareType.cs` (Miner/Bot/Spammer/Ransomware), `ToolType.cs` (CrackWEP/CrackWPA/CrackWPA2/FirewallDisable), `ContractType.cs` (DDOS/FileRetrieval/FacilitatedAttack), `FirewallStatus.cs` (Active/Disabled), `StoreItemCategory.cs` (PCComponent/Software)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core models, save system, tick system, and command parser that ALL user
stories depend on. No user story work can begin until this phase is complete.

**⚠️ CRITICAL**: No user story implementation can start until Phase 2 is complete.

### Core Models

- [X] T009 [P] Create `Location` model in `Assets/Scripts/Models/Location.cs` — fields: id (string), configId (string), networks (List\<Network\>), isVisited (bool)
- [X] T010 [P] Create `Network` model in `Assets/Scripts/Models/Network.cs` — fields: ssid (string), securityLevel (SecurityLevel), devices (List\<Device\>), isHacked (bool), isScanned (bool)
- [X] T011 [P] Create `Device` model in `Assets/Scripts/Models/Device.cs` — fields: ip (string), mac (string), firewallStatus (FirewallStatus), openPorts (List\<int\>), activeMalware (Malware, nullable), isScanned (bool)
- [X] T012 [P] Create `Malware` model in `Assets/Scripts/Models/Malware.cs` — fields: type (MalwareType), deviceIp (string), incomeRate (double), currency (CurrencyType), installedAtUtcTicks (long)
- [X] T013 Create `Player` model in `Assets/Scripts/Models/Player.cs` — fields: balances (List\<CurrencyBalance\>), unlockedTools (List\<ToolType\>), unlockedCurrencies (List\<CurrencyType\>), purchasedItems (List\<string\>), activeContracts (List\<Contract\>), completedContracts (List\<string\>), lastSaveUtcTicks (long); include `CurrencyBalance` nested class (CurrencyType + double) for JsonUtility compat
- [X] T014 Create `SaveData` aggregate in `Assets/Scripts/Models/SaveData.cs` — fields: player (Player), locations (List\<LocationSaveData\>), version (int); include `LocationSaveData`, `NetworkSaveData`, `DeviceSaveData` nested classes

### Core Infrastructure Tests (write first — must FAIL before implementing)

- [X] T015 [P] Write `CommandParserTests` in `Assets/Tests/EditMode/CommandParserTests.cs` — test: unknown command returns error message; known command is dispatched to correct handler; args are tokenized correctly; case-insensitive matching
- [X] T016 [P] Write `SaveSystemTests` in `Assets/Tests/EditMode/SaveSystemTests.cs` — test: SaveData serializes to JSON without loss; loading non-existent file returns default SaveData; round-trip (save then load) preserves all fields
- [X] T017 [P] Write `OfflineIncomeCalculatorTests` in `Assets/Tests/EditMode/OfflineIncomeCalculatorTests.cs` — test: zero elapsed time yields zero income; 60 seconds with 1.0/s rate yields 60.0; multiple miners accumulate correctly; no active miners yields zero

### Core Infrastructure Implementation

- [X] T018 Implement `CommandParser` in `Assets/Scripts/Services/CommandParser.cs` — Dictionary\<string, ICommand\> registry; tokenize input by whitespace; first token = verb; remaining = args; pass to registered handler; return "Unknown command" error for unregistered verbs; do NOT use LINQ in the parse path (per Unity perf guidelines — cache and reuse arrays); (verify T015 passes)
- [X] T019 Implement `SaveSystem` in `Assets/Scripts/Core/SaveSystem.cs` — serialize SaveData to `Application.persistentDataPath/save.json` via JsonUtility; load on startup; return default SaveData if file missing (verify T016 passes)
- [X] T020 Implement `TickManager` in `Assets/Scripts/Core/TickManager.cs` — MonoBehaviour using `InvokeRepeating` at 1-second interval (NOT per-frame Update); maintains list of ITickable subscribers; calls `OnTick(1.0)` on each registered subscriber
- [X] T021 Implement `OfflineIncomeCalculator` in `Assets/Scripts/Core/OfflineIncomeCalculator.cs` — for each active Malware compute `baselineTicks = Math.Max(malware.installedAtUtcTicks, player.lastSaveUtcTicks)`, then `elapsedSeconds = (DateTime.UtcNow.Ticks - baselineTicks) / TimeSpan.TicksPerSecond`, then `income += elapsedSeconds × malware.incomeRate`; accumulate per CurrencyType; returns Dictionary-equivalent result (verify T017 passes)
- [X] T022 [P] Create `CurrencyDefinitionSO` ScriptableObject in `Assets/Scripts/Data/CurrencyDefinitionSO.cs` — fields: currencyType (CurrencyType), displayName (string), symbol (string)
- [X] T023 [P] Create `LocationConfigSO` ScriptableObject in `Assets/Scripts/Data/LocationConfigSO.cs` — fields: minNetworks (int), maxNetworks (int), securityDistribution (SecurityLevel[] weighted list), minDevicesPerNetwork (int), maxDevicesPerNetwork (int)
- [X] T024 Implement `LocationService` in `Assets/Scripts/Services/LocationService.cs` — generates Location with Networks and Devices from LocationConfigSO; uses deterministic seed per location id; exposes `GetOrCreateLocation(id)` and `MoveToNextLocation()`
- [X] T025 Create `GameManager` in `Assets/Scripts/Core/GameManager.cs` — MonoBehaviour singleton; on Awake: load SaveData via SaveSystem, apply offline income via OfflineIncomeCalculator, register all ITickable services to TickManager; on ApplicationQuit/OnApplicationPause: save via SaveSystem

**Checkpoint**: Foundation complete — all edit-mode tests pass, game bootstraps and saves/loads. User story work can begin.

---

## Phase 3: User Story 1 — First Network Infiltration (Priority: P1) 🎯 MVP

**Goal**: Player can scan for networks, crack one, scan a device, disable its firewall, inject a miner, and see passive income accumulate. Offline income applies on next load.

**Independent Test**: Run quickstart.md Steps 1–8. Income visible within 30 seconds. Offline income applied after simulated close/reopen.

### Tests for User Story 1 ⚠️ Write first — must FAIL before implementing

- [X] T026 [P] [US1] Write `ScanCommandTests` in `Assets/Tests/EditMode/ScanCommandTests.cs` — test: first scan returns network list; second scan moves location; `scan network {SSID}` returns security level and devices; `scan ip {IP}` returns firewall status and ports; unknown SSID returns error per command-schema.md
- [X] T027 [P] [US1] Write `CrackCommandTests` in `Assets/Tests/EditMode/CrackCommandTests.cs` — test: correct tool + correct security level grants access; wrong tool type returns mismatch error; missing tool returns "purchase required" error; already-hacked network returns error
- [X] T028 [P] [US1] Write `FirewallCommandTests` in `Assets/Tests/EditMode/FirewallCommandTests.cs` — test: disable on active firewall succeeds; enable on disabled firewall succeeds; disable on already-disabled returns error; no targeted device returns error; no tool returns error
- [X] T029 [P] [US1] Write `InjectCommandTests` (miner only) in `Assets/Tests/EditMode/InjectCommandTests.cs` — test: inject miner on accessible device with disabled firewall succeeds; firewall active blocks injection; already-infected device returns error; no targeted device returns error
- [X] T030 [P] [US1] Write `IncomeServiceTests` in `Assets/Tests/EditMode/IncomeServiceTests.cs` — test: OnTick adds `incomeRate × deltaSeconds` to player balance per active miner; multiple miners accumulate; zero miners produces no change; correct currency credited

### Implementation for User Story 1

- [X] T031 [P] [US1] Implement `ScanCommand` in `Assets/Scripts/Services/Commands/ScanCommand.cs` — implements ICommand; handles `scan`, `scan network {SSID}`, `scan ip {IP}`, `scan mac {MAC}`; delegates to LocationService; formats output per command-schema.md; tracks currently targeted device on Player session state (verify T026 passes)
- [X] T032 [P] [US1] Implement `CrackCommand` in `Assets/Scripts/Services/Commands/CrackCommand.cs` — validates tool ownership from Player.unlockedTools; validates security level match; sets Network.isHacked = true on success; output per command-schema.md (verify T027 passes)
- [X] T033 [P] [US1] Implement `FirewallCommand` in `Assets/Scripts/Services/Commands/FirewallCommand.cs` — requires FirewallDisable tool; toggles Device.firewallStatus; output per command-schema.md (verify T028 passes)
- [X] T034 [P] [US1] Implement `ShowCommand` in `Assets/Scripts/Services/Commands/ShowCommand.cs` — handles `show networks` and `show ips`; reads from Player/LocationService state; output per command-schema.md
- [X] T035 [US1] Implement `IncomeService` in `Assets/Scripts/Services/IncomeService.cs` — implements ITickable; on each tick iterates active Miner/Spammer malware with a `for` loop (no LINQ — Unity docs prohibit LINQ in hot paths to avoid GC allocations); accumulates income per CurrencyType; credits Player.balances; registered with TickManager by GameManager (verify T030 passes)
- [X] T036 [US1] Implement `InjectCommand` (Miner only) in `Assets/Scripts/Services/Commands/InjectCommand.cs` — validates firewall status and existing infection; creates Malware with MalwareType.Miner, sets installedAtUtcTicks; attaches to Device; output per command-schema.md (verify T029 passes)
- [X] T037 [US1] Register ScanCommand, CrackCommand, FirewallCommand, ShowCommand, InjectCommand in `CommandParser` via `GameManager.Awake()`

### UI for User Story 1

- [X] T038 [P] [US1] Create `TerminalOutputView` in `Assets/Scripts/UI/TerminalOutputView.cs` — MonoBehaviour; scrollable TextMeshPro output panel; exposes `AppendLine(string)` and `Clear()`; use a `StringBuilder` (cached, not reallocated per call) to build output text and avoid per-append GC allocations; auto-scrolls to bottom on new output
- [X] T039 [US1] Create `TerminalController` in `Assets/Scripts/UI/TerminalController.cs` — MonoBehaviour; wires InputField submit event to `CommandParser.Parse(input)`; passes CommandResult.message to `TerminalOutputView.AppendLine`; clears input field after submit
- [X] T040 [US1] Create `HUDController` in `Assets/Scripts/UI/HUDController.cs` — MonoBehaviour; subscribes to Player balance changes; displays current BTC balance via TextMeshPro; updates every TickManager tick
- [ ] T041 [US1] Create `Terminal.prefab` in `Assets/Prefabs/Terminal.prefab` — contains Canvas, InputField, TerminalOutputView scroll panel, TerminalController; set up anchors for full-screen terminal layout ⚠️ REQUIRES UNITY EDITOR
- [ ] T042 [US1] Set up `Assets/Scenes/GameScene.unity` — add Terminal.prefab, HUD.prefab; add GameManager, TickManager GameObjects; verify scene runs without Console errors in Play Mode ⚠️ REQUIRES UNITY EDITOR

### Integration Test

- [X] T043 [US1] Write `CoreLoopIntegrationTests` in `Assets/Tests/PlayMode/CoreLoopIntegrationTests.cs` — play-mode test: simulate scan → crack → inject miner sequence via CommandParser; assert Player balance increases after 2 tick intervals; for offline income: call `SaveSystem.Save()` directly, then call a test-only `GameManager.SimulateLoad()` that re-runs startup logic (do NOT use `Application.Quit()` — no-op in Editor), assert offline income is applied proportional to a mocked elapsed time

**Checkpoint**: User Story 1 fully functional. Run quickstart.md Steps 1–8 to validate independently.

---

## Phase 4: User Story 2 — Upgrade and Expand (Priority: P2)

**Goal**: Player can open the store, see items with prices, purchase upgrades, and have effects (income multiplier, new tools) applied immediately. Progressive currency unlock works.

**Independent Test**: Run quickstart.md Step 9. Balance deducted correctly, upgrade active after purchase.

### Tests for User Story 2 ⚠️ Write first — must FAIL before implementing

- [X] T044 [P] [US2] Write `StoreServiceTests` in `Assets/Tests/EditMode/StoreServiceTests.cs` — test: affordable item deducts balance and marks as purchased; unaffordable item returns insufficient-funds error; already-purchased item returns error; software item adds correct ToolType to Player.unlockedTools; PC component item updates income multiplier; currency-unlock item adds new CurrencyType to Player.unlockedCurrencies

### Models for User Story 2

- [X] T045 [P] [US2] Create `StoreItem` model in `Assets/Scripts/Models/StoreItem.cs` — fields: id (string), displayName (string), description (string), category (StoreItemCategory), price (double), priceCurrency (CurrencyType), unlocksToolType (ToolType, nullable), unlocksCurrency (CurrencyType, nullable), incomeMultiplier (double)
- [X] T046 [P] [US2] Create `StoreItemSO` ScriptableObject in `Assets/Scripts/Data/StoreItemSO.cs` — mirrors StoreItem fields; used by designers to author store catalog without code changes

### Implementation for User Story 2

- [X] T047 [US2] Implement `StoreService` in `Assets/Scripts/Services/StoreService.cs` — validates purchase (balance, not already owned); deducts balance; applies effect (add ToolType, add CurrencyType, set income multiplier on IncomeService); persists purchase to Player.purchasedItems (verify T044 passes)
- [X] T048 [P] [US2] Extend `InjectCommand` in `Assets/Scripts/Services/Commands/InjectCommand.cs` to handle Bot, Spammer, Ransomware malware types — bot enables contract availability; spammer adds passive income; ransomware sets one-time demand; all check software ownership via Player.unlockedTools
- [ ] T049 [US2] Create starter StoreItemSO assets in `Assets/Resources/StoreItems/` — minimum 5 items: WEP crack tool, WPA crack tool, firewall disable tool, PC component (×1.5 income), software (bot injection); set prices achievable after first successful miner run ⚠️ REQUIRES UNITY EDITOR
- [X] T050 [US2] Create `StoreController` in `Assets/Scripts/UI/StoreController.cs` — MonoBehaviour; reads StoreItemSO assets from Resources; renders item list via TextMeshPro; handles purchase button events; delegates to StoreService; shows success/error feedback in terminal output

**Checkpoint**: User Stories 1 and 2 both independently functional. Run quickstart.md Steps 9.

---

## Phase 5: User Story 3 — Accept and Complete Contracts (Priority: P3)

**Goal**: Player with active bot injection can view contracts, accept one, complete its objectives, and receive a reward.

**Independent Test**: Run quickstart.md Step 10. Contract marked complete, reward credited.

### Tests for User Story 3 ⚠️ Write first — must FAIL before implementing

- [X] T051 [P] [US3] Write `ContractServiceTests` in `Assets/Tests/EditMode/ContractServiceTests.cs` — test: contracts unavailable without bot; contracts listed after bot injection; accepting contract adds to Player.activeContracts; completing all objectives marks contract complete and credits reward; contract requiring missing capability returns prerequisite error

### Models for User Story 3

- [X] T052 [P] [US3] Create `Contract` and `Objective` models in `Assets/Scripts/Models/Contract.cs` — Contract fields: id (string), type (ContractType), description (string), requiredMalware (MalwareType, nullable), objectives (List\<Objective\>), reward (double), rewardCurrency (CurrencyType), isCompleted (bool); Objective fields: description (string), isComplete (bool)

### Implementation for User Story 3

- [X] T053 [US3] Implement `ContractService` in `Assets/Scripts/Services/ContractService.cs` — availability gated by Player malware injection status; accept adds contract to Player.activeContracts; objective completion tracked per step; on full completion: marks contract, credits reward, moves to Player.completedContracts (verify T051 passes)
- [ ] T054 [US3] Create starter contract assets in `Assets/Resources/Contracts/` — minimum 3 contracts: one DDOS (requires bot), one file retrieval (requires scan + copy), one facilitated attack (requires firewall disable + bot); rewards achievable within a normal play session ⚠️ REQUIRES UNITY EDITOR
- [X] T055 [US3] Implement `LsCommand` in `Assets/Scripts/Services/Commands/LsCommand.cs` and `CopyCommand` in `Assets/Scripts/Services/Commands/CopyCommand.cs` — per command-schema.md; LsCommand reads `Device.files` (List\<DeviceFile\>) for the targeted device; CopyCommand copies selected file path to `Player.copiedFiles`; required for file-retrieval contracts; register both in GameManager
- [X] T056 [US3] Create contracts panel UI — MonoBehaviour `ContractController` in `Assets/Scripts/UI/ContractController.cs`; lists available and active contracts; accept/view buttons; delegates to ContractService; shows reward on completion
- [X] T057 [US3] Write `StoreContractIntegrationTests` in `Assets/Tests/PlayMode/StoreContractIntegrationTests.cs` — play-mode test: purchase bot software → verify contract available → accept DDOS contract → complete objectives → verify reward credited to Player balance

**Checkpoint**: All three user stories independently functional. Run quickstart.md Step 10.

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Quality hardening across all stories per Constitution principles.

- [X] T058 [P] Add XML documentation comments to all public classes and methods in `Assets/Scripts/` (Constitution Principle I — all public APIs must have XML docs)
- [X] T059 Add `ProfilerRecorder` (Unity 6 built-in — `Unity.Profiling.ProfilerRecorder`) to a test MonoBehaviour that measures main-thread frame time with 10+ active miners over 60 ticks; assert average frame time < 22.22 ms; remove recorder in `OnDisable()` to free unmanaged resources; attach recorded data to PR (Constitution Principle IV)
- [X] T064 Run Unity Profiler against LsCommand and CopyCommand with a device containing 100+ files; use `ProfilerRecorder` to capture GC allocations during command execution; confirm zero per-call GC in hot path; attach results to PR (Constitution Principle IV)
- [ ] T060 [P] Audit all command output strings against `specs/001-hacking-idle-core/contracts/command-schema.md`; fix any mismatches (Constitution Principle III)
- [ ] T061 [P] Create `Bitcoin` CurrencyDefinitionSO asset in `Assets/Resources/Currencies/Bitcoin.asset`; create LocationConfigSO asset for default location in `Assets/Resources/Locations/DefaultLocation.asset` ⚠️ REQUIRES UNITY EDITOR
- [ ] T065 [P] Create one altcoin `CurrencyDefinitionSO` asset (e.g., `Assets/Resources/Currencies/Monero.asset`) and one corresponding `StoreItemSO` in `Assets/Resources/StoreItems/` with `unlocksCurrency = CurrencyType.Monero` at a milestone price; add `CurrencyType.Monero` to the `CurrencyType` enum (FR-016) ⚠️ REQUIRES UNITY EDITOR (enum already updated)
- [ ] T062 Run full `specs/001-hacking-idle-core/quickstart.md` end-to-end validation in Play Mode; document results ⚠️ REQUIRES UNITY EDITOR
- [ ] T063 [P] Code review pass — verify no magic numbers (replace with named constants, including ransom amount from LocationConfigSO), no nesting deeper than 3 levels, no dead code committed (Constitution Principle I)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — **BLOCKS all user stories**
- **US1 (Phase 3)**: Depends on Phase 2 completion
- **US2 (Phase 4)**: Depends on Phase 2; integrates with US1 (StoreService updates IncomeService multiplier)
- **US3 (Phase 5)**: Depends on Phase 2; requires Bot injection from US2 to be complete
- **Polish (Phase N)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — no dependency on US2 or US3
- **US2 (P2)**: Can start after Phase 2 — integrates with US1's IncomeService but does not block it
- **US3 (P3)**: Can start after Phase 2 — requires bot injection feature from US2

### Within Each User Story

1. Tests MUST be written and confirmed **failing** before implementation (Red-Green-Refactor)
2. Models before services
3. Services before UI
4. Commands before integration tests
5. Story complete and checkpointed before moving to next priority

### Parallel Opportunities

- All Phase 1 tasks marked [P] can run in parallel
- T009–T012 (core models) can run in parallel
- T015–T017 (foundational tests) can run in parallel
- T026–T030 (US1 tests) can all be written in parallel
- T031–T034 (US1 commands) can be implemented in parallel after their tests pass

---

## Parallel Example: User Story 1 Tests

```text
Parallel batch — write all US1 tests together:
  T026: ScanCommandTests
  T027: CrackCommandTests
  T028: FirewallCommandTests
  T029: InjectCommandTests (miner)
  T030: IncomeServiceTests

Then parallel batch — implement commands (each in separate file):
  T031: ScanCommand
  T032: CrackCommand
  T033: FirewallCommand
  T034: ShowCommand
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational — **CRITICAL, blocks everything**
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Run quickstart.md Steps 1–8
5. Ship/demo the core hacking loop

### Incremental Delivery

1. Phase 1 + Phase 2 → Foundation ready
2. Phase 3 → Core loop (scan/crack/inject/income) ✅ Demo
3. Phase 4 → Store + upgrades ✅ Demo
4. Phase 5 → Contracts ✅ Demo
5. Phase N → Polished, shippable build

---

## Notes

- `[P]` tasks touch different files — safe to parallelise
- `[US#]` label maps task to a specific user story for traceability
- Each user story is independently completable and testable
- Tests MUST fail before implementation starts (Constitution Principle II)
- Commit after each task or logical group
- Stop at each checkpoint to validate story independently
- Constitution Principle IV: attach Profiler data to any PR introducing new Update() paths
