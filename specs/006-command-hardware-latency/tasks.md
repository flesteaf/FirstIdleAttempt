# Tasks: Command Hardware Latency

**Input**: Design documents from `specs/006-command-hardware-latency/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/command-schema.md

---

## ⚠️ Spec Divergence Notice

`plan.md`, `data-model.md`, and `contracts/command-schema.md` were generated before the spec was revised to the **bottleneck model** (FR-010, FR-011, FR-021). **These tasks reflect the current `spec.md` and take precedence.** Where a task conflicts with those documents, follow the task.

| Document says | This file uses instead |
|---|---|
| `Player.InternetTier` | `Player.BandwidthTier` |
| `HardwareStat.Internet = 1` | `HardwareStat.Bandwidth = 1` |
| `inject` uses player CPU tier + target CPU resistance | `inject` time = `min(player_bw, target_bw)` only |
| `firewall` uses target CPU resistance | `firewall` time = `min(player_bw, target_bw)` only |
| `scan ip/mac` uses `TargetBwScanOverheadTable` | `scan ip/mac` uses `min(player_bw, target_bw)` |
| `TargetCpuResistanceTable` coefficient | **Removed** |
| `TargetBwResistanceTable` coefficient | **Removed** |
| `TargetBwScanOverheadTable` coefficient | **Removed** |
| `TargetBwSpeedupTable` (separate table) | **Merged** into single `BandwidthSpeedupTable` |
| Shop assets `hw-net-*` | Shop assets `hw-bw-*` |
| CPU shop descriptions: "crack, inject" | CPU shop descriptions: "crack" only |
| No payload yield task | T031 wires target CPU tier → miner yield (FR-022, FR-023) |

---

## Phase 1: Setup

No project initialization required — this feature branches off an existing Unity 6 project.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Model layer, enum, service creation, and GameManager wiring. Everything here MUST be complete before any user story phase begins.

**⚠️ CRITICAL**: T006, T007, and T009 must be complete before Phase 3.

- [ ] T001 [P] Create `Assets/Scripts/Models/HardwareStat.cs` with `public enum HardwareStat { CPU = 0, Bandwidth = 1, GPU = 2 }`
- [ ] T002 [P] Add `public int CpuTier`, `public int BandwidthTier`, `public int GpuTier` to `Assets/Scripts/Models/Player.cs`; delete `CommandSpeedUpgrade`; temporarily stub any compile error from `GameManager.GetCommandLatency()` as `0f` until T009
- [ ] T003 [P] Add `public int CpuTier` and `public int BandwidthTier` to `Assets/Scripts/Models/Device.cs`
- [ ] T004 [P] Add `public int CpuTier` and `public int BandwidthTier` to `DeviceSaveData` in `Assets/Scripts/Models/SaveData.cs`; bump `SaveData.Version` constant to `3`
- [ ] T005 [P] Add `[Header("Hardware Upgrade")]` section to `Assets/Scripts/Data/StoreItemSO.cs` with `bool HasHardwareUpgrade`, `HardwareStat HardwareStatAffected`, `int HardwareTierGranted`; mirror fields in `Assets/Scripts/Models/StoreItem.cs`; extend `StoreItemSO.ToModel()` to map all three (depends on T001)
- [ ] T006 Create `Assets/Scripts/Services/CommandLatencyService.cs`. Constructor: `CommandLatencyService(Player player)`. Three named coefficient arrays (no inline literals): `CpuSpeedupTable = {1.0f,1.5f,2.5f,3.5f,5.0f}` (index = tier−1); `GpuSpeedupTable = {1.0f,1.6f,2.8f,4.0f}` (index = tier); `BandwidthSpeedupTable = {1.0f,1.5f,2.3f,3.2f,4.0f}` (index = tier−1, shared by player and target). Named constants: base/cap per verb (`BaseCrackWEP=3.0f`, `MaxCrackWEP=10.0f`, etc.), `MinFloor=0.05f`, `CopyReferenceSizeBytes=10_000_000L`, `CopyScaleFactor=2.0f`. `CommandLatencyContext` readonly struct in same file: `CommandVerb`, `SecurityLevel`, `TargetDevice`, `FileSizeBytes`. `public float CalculateLatency(CommandLatencyContext ctx)` routes to private helpers: **crack** = `Clamp(base / (CpuSpeedupTable[player.CpuTier−1] × GpuSpeedupTable[player.GpuTier]), MinFloor, cap)`; **all network commands** derive `effectiveBw = BandwidthSpeedupTable[Math.Min(player.BandwidthTier, ctx.TargetDevice?.BandwidthTier ?? 1) − 1]` then `Clamp(base / effectiveBw, MinFloor, cap)`; **copy** multiplies base by `(1f + ctx.FileSizeBytes / (float)CopyReferenceSizeBytes × CopyScaleFactor)` before dividing by `effectiveBw` (depends on T002, T003)
- [ ] T007 [P] Add default interface method `float GetLatency(string[] args) => 0f;` to `Assets/Scripts/Interfaces/ICommand.cs`
- [ ] T008 Extend `GameManager.MigrateIfNeeded()` in `Assets/Scripts/Core/GameManager.cs` for v2→v3: if `player.CpuTier == 0` set to `1`; if `player.BandwidthTier == 0` set to `1`; for each `DeviceSaveData` in all networks: if `CpuTier == 0` set to `1`, if `BandwidthTier == 0` set to `1`; bump migration guard version to `3` (depends on T002, T004)
- [ ] T009 Add `public CommandLatencyService CommandLatencyService { get; private set; }` to `Assets/Scripts/Core/GameManager.cs`; in `Bootstrap()` after Player is loaded add `CommandLatencyService = new CommandLatencyService(SaveData.Player)`; remove `GetCommandLatency()` and `BaseCommandLatencySeconds` — compile errors from those callers are resolved in T018–T019 (depends on T006, T008)
- [ ] T010 Add hardware tier branch to `StoreService.ApplyEffect()` in `Assets/Scripts/Services/StoreService.cs`: when `item.HasHardwareUpgrade`, switch on `item.HardwareStatAffected` and set `_player.CpuTier`, `_player.BandwidthTier`, or `_player.GpuTier` = `item.HardwareTierGranted`; guard: only apply if `HardwareTierGranted > currentTier` (depends on T001, T002, T005)

**Checkpoint**: Foundation complete — T006, T007, T009, T010 done. User story phases can begin.

---

## Phase 3: User Story 1 — Commands take realistic, variable time (Priority: P1) 🎯 MVP

**Goal**: Replace flat 1-second universal delay with per-command base times shaped by player hardware. Progress bar durations vary meaningfully across commands and hardware tiers.

**Independent Test**: Base hardware (CPU1, BW1, GPU0). Run `crack WPA2` (≈10s), `ls` (≈0.5s), `show` (instant, no bar). Purchase CPU tier-2 asset — rerun `crack WPA2` and observe a shorter bar. `scan` and `ls` times must be unchanged by the CPU purchase.

- [ ] T011 [P] [US1] Override `GetLatency` in `Assets/Scripts/Services/Commands/CrackCommand.cs`: add `CommandLatencyService latencyService` as final constructor param (null-safe: return `0f` when null); parse `SecurityLevel` from `args[0]`; return `latencyService.CalculateLatency(new CommandLatencyContext { CommandVerb="crack", SecurityLevel=secLevel })`
- [ ] T012 [P] [US1] Override `GetLatency` in `Assets/Scripts/Services/Commands/ScanCommand.cs`: add `CommandLatencyService latencyService` constructor param; detect area vs ip/mac variant from `args`; for ip/mac resolve `Device` from `LocationService` by address arg; return `latencyService.CalculateLatency(ctx)` with appropriate verb and target — time for ip/mac is `min(player.BandwidthTier, target.BandwidthTier)`; no overhead table
- [ ] T013 [P] [US1] Override `GetLatency` in `Assets/Scripts/Services/Commands/InjectCommand.cs`: add `CommandLatencyService latencyService` constructor param; resolve target `Device` by IP from `args[0]`; return `latencyService.CalculateLatency(new CommandLatencyContext { CommandVerb="inject", TargetDevice=device })` — time is governed solely by `min(player.BandwidthTier, target.BandwidthTier)`; player CPU and target CPU tier do NOT affect time
- [ ] T014 [P] [US1] Override `GetLatency` in `Assets/Scripts/Services/Commands/FirewallCommand.cs`: add `CommandLatencyService latencyService` constructor param; resolve target `Device`; return `latencyService.CalculateLatency(ctx)` with `CommandVerb="firewall"` — target CPU tier does NOT affect time
- [ ] T015 [P] [US1] Override `GetLatency` in `Assets/Scripts/Services/Commands/LsCommand.cs`: add `CommandLatencyService latencyService` constructor param; resolve target `Device` from args or active selection; return `latencyService.CalculateLatency(ctx)` — if device unresolvable at pre-execution time pass `TargetDevice=null` (service defaults to BW tier 1)
- [ ] T016 [P] [US1] Override `GetLatency` in `Assets/Scripts/Services/Commands/CopyCommand.cs`: add `CommandLatencyService latencyService` constructor param; resolve target `Device` and `DeviceFile` from args; return `latencyService.CalculateLatency(new CommandLatencyContext { CommandVerb="copy", TargetDevice=device, FileSizeBytes=file?.SizeBytes ?? 0 })`
- [ ] T017 [P] [US1] Add `public ICommand GetCommand(string verb)` to `Assets/Scripts/Services/CommandParser.cs`: returns registered command or `null`; add XML doc comment (can be done in parallel with T011–T016)
- [ ] T018 [US1] Replace `GameManager.Instance.GetCommandLatency()` in `Assets/Scripts/UI/TerminalController.cs` `OnSubmit`: parse verb, call `CommandParser.GetCommand(verb)`, build `args` array, call `cmd.GetLatency(args)` for `latency`; if `cmd == null` use `0f` (depends on T011–T017)
- [ ] T019 [US1] Update `GameManager.RegisterCommands()` in `Assets/Scripts/Core/GameManager.cs` to pass `CommandLatencyService` as final constructor argument to CrackCommand, ScanCommand, InjectCommand, FirewallCommand, LsCommand, CopyCommand (depends on T009, T011–T016)
- [ ] T020 [US1] Create `Assets/Tests/EditMode/CommandLatencyServiceTests.cs`: (a) base player returns each command's named base time; (b) effective time never drops below `0.05f`; (c) CPU5+GPU3 crack = base/20; (d) inject with player-BW5 vs target-BW1 equals inject with player-BW1 vs target-BW1 (player is bottleneck test); (e) inject player-BW5 vs target-BW5 is faster than vs target-BW1 (high-BW target removes bottleneck); (f) inject time with target-CPU1 equals inject time with target-CPU5 given identical BW tiers (CPU does not affect time); (g) copy scales proportionally with file size (depends on T006)

**Checkpoint**: User Story 1 done. Every command shows a variable-length progress bar; `show`/`help`/`forget`/`move` remain instant.

---

## Phase 4: User Story 2 — Player CPU upgrades (Priority: P2)

**Goal**: Purchasing CPU tier shop items measurably reduces `crack` duration. `scan`, `ls`, and all bandwidth-bound commands are unaffected.

**Independent Test**: Purchase `hw-cpu-2` (Dual-Core CPU). Run `crack WPA2` before and after — bar must be shorter. Run `scan` before and after — time must be identical.

- [ ] T021 [P] [US2] Create 4 CPU `StoreItemSO` assets in `Assets/Resources/StoreItems/`: `hw-cpu-2.asset` (Dual-Core CPU, CPU tier 2, 0.0050 BTC, description: "Speeds up crack"), `hw-cpu-3.asset` (Quad-Core CPU, tier 3, 0.0150 BTC), `hw-cpu-4.asset` (Workstation CPU, tier 4, 0.0500 BTC), `hw-cpu-5.asset` (Server Rack, tier 5, 0.1500 BTC); all with `HasHardwareUpgrade=true`, `HardwareStatAffected=CPU`, `Category=PCComponent`
- [ ] T022 [US2] Add CPU hardware tier tests to `Assets/Tests/EditMode/StoreServiceTests.cs`: purchase CPU tier-2 item → `player.CpuTier == 2`; purchase lower tier than current → tier unchanged; purchase same item twice → tier unchanged (depends on T010, T021)

**Checkpoint**: CPU upgrades purchasable and verifiably reduce crack time.

---

## Phase 5: User Story 3 — Player bandwidth upgrades (Priority: P2)

**Goal**: Purchasing bandwidth tier shop items measurably reduces all network-bound command durations. `crack` is unaffected.

**Independent Test**: Purchase `hw-bw-2` (Cable Internet). Run `scan` before and after — bar must be shorter. Run `crack` before and after — time must be identical. Demonstrate bottleneck: upgrade to BW5, then copy a file from a BW1 device — speed must be capped at BW1 regardless.

- [ ] T023 [P] [US3] Create 4 bandwidth `StoreItemSO` assets in `Assets/Resources/StoreItems/`: `hw-bw-2.asset` (Cable Internet, Bandwidth tier 2, 0.0030 BTC, description: "Speeds up scan, inject, firewall, ls, copy"), `hw-bw-3.asset` (Fibre Lite, tier 3, 0.0100 BTC), `hw-bw-4.asset` (Full Fibre, tier 4, 0.0300 BTC), `hw-bw-5.asset` (Dedicated Line, tier 5, 0.1000 BTC); all with `HasHardwareUpgrade=true`, `HardwareStatAffected=Bandwidth`, `Category=PCComponent`
- [ ] T024 [US3] Add bandwidth tier tests to `Assets/Tests/EditMode/StoreServiceTests.cs`: purchase BW tier-2 item → `player.BandwidthTier == 2`; purchase lower tier than current → tier unchanged (depends on T010, T023)

**Checkpoint**: Bandwidth upgrades purchasable and verifiably reduce network command times.

---

## Phase 6: User Story 4 — Player GPU upgrades (Priority: P3)

**Goal**: Purchasing GPU tier shop items provides a dramatic reduction to `crack` exclusively. No effect on any other command.

**Independent Test**: Purchase `hw-gpu-1`. Run `crack WPA2` before and after — bar must be shorter than the equivalent single CPU tier jump. Run `ls` before and after — time must be identical.

- [ ] T025 [P] [US4] Create 3 GPU `StoreItemSO` assets in `Assets/Resources/StoreItems/`: `hw-gpu-1.asset` (Basic Gaming GPU, GPU tier 1, 0.0080 BTC, description: "Speeds up crack only"), `hw-gpu-2.asset` (Workstation GPU, tier 2, 0.0250 BTC), `hw-gpu-3.asset` (Compute Cluster GPU, tier 3, 0.0800 BTC); all with `HasHardwareUpgrade=true`, `HardwareStatAffected=GPU`, `Category=PCComponent`
- [ ] T026 [US4] Add GPU tier tests to `Assets/Tests/EditMode/StoreServiceTests.cs`: purchase GPU tier-1 → `player.GpuTier == 1`; purchase lower GPU tier than current → unchanged (depends on T010, T025)

**Checkpoint**: GPU upgrades purchasable; crack visibly faster; no other command affected.

---

## Phase 7: User Story 5 — Target device hardware (Priority: P3)

**Goal**: Target device bandwidth tier acts as the other end of the network connection — high-bandwidth targets allow faster transfers when the player's connection is not the bottleneck. Target CPU tier drives injected payload output rate, not execution time.

**Independent Test**: Find two devices — BW1 and BW4 (via `scan ip`). With player BW3, run `copy` on both — the BW4 device must copy faster. Run `inject miner` on both — times must be identical (same player+target BW on both paths means same min). With two CPU1 and CPU5 targets injected with a miner, verify CPU5 target produces more mining yield after waiting.

- [ ] T027 [P] [US5] Append per-device tier generation at the end of the inner device loop in `LocationService.GenerateLocation()` in `Assets/Scripts/Services/LocationService.cs`: `device.CpuTier = rng.Next(1, 6); device.BandwidthTier = rng.Next(1, 6);` — placed AFTER all existing `rng.Next()` calls to preserve existing seed sequences
- [ ] T028 [P] [US5] In `LocationService.ToSaveData()` copy `dev.CpuTier` → `DeviceSaveData.CpuTier` and `dev.BandwidthTier` → `DeviceSaveData.BandwidthTier` for each device in `Assets/Scripts/Services/LocationService.cs`
- [ ] T029 [P] [US5] In `LocationService.RestoreFromSave()` set `dev.CpuTier = ds.CpuTier` and `dev.BandwidthTier = ds.BandwidthTier`; treat `0` as `1` as a fallback if migration has not yet run in `Assets/Scripts/Services/LocationService.cs`
- [ ] T030 [US5] In `ScanCommand.Execute()` in `Assets/Scripts/Services/Commands/ScanCommand.cs`, add to the scan ip/mac output block (after existing device properties, guard: `device.IsScanned`): `CPU Tier: {device.CpuTier}/5` and `BW Tier:  {device.BandwidthTier}/5` on separate lines (depends on T027)
- [ ] T031 [US5] In `InjectCommand.Execute()` in `Assets/Scripts/Services/Commands/InjectCommand.cs`, pass `device.CpuTier` to the injected payload when creating or registering the miner: the miner's yield rate (e.g. Bitcoin-per-second in `IncomeService` or equivalent) MUST scale with the target's `CpuTier` so that a tier-5 target produces more output than a tier-1 target (FR-022, FR-023) — locate the existing yield/income calculation and substitute the target CPU tier factor in place of any flat rate (depends on T013, T030)

**Checkpoint**: Devices show hardware tiers after scan. Copy/ls faster against high-BW targets when player BW is sufficient. Miner yield visibly higher on high-CPU targets.

---

## Phase 8: Polish & Cross-Cutting Concerns

- [ ] T032 [P] Update `HelpCommand` registered entries in `Assets/Scripts/Services/Commands/HelpCommand.cs` to note that hardware upgrades affect command speed; CPU → crack only; Bandwidth → all network commands; GPU → crack only
- [ ] T033 [P] Add XML doc comments to all `public` members of `Assets/Scripts/Services/CommandLatencyService.cs` (constitution §I compliance)
- [ ] T034 [P] Add XML doc comment to `CommandParser.GetCommand()` in `Assets/Scripts/Services/CommandParser.cs`
- [ ] T035 Manual UX gate — before PR merge, verify in Unity Play Mode: `crack WPA2` bar noticeably longer than `ls`; purchasing CPU upgrade makes crack bar shorter; `scan ip` on a scanned device shows CPU Tier and BW Tier lines; `show` and `help` remain instant; copy from a high-BW device faster than low-BW device when player BW exceeds target BW; include terminal output screenshots in PR description

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies on user stories — **BLOCKS all phases below**
- **US1 (Phase 3)**: Depends on Foundational (T006, T007, T009) — can begin immediately after checkpoint
- **US2, US3, US4 (Phases 4–6)**: Depend on Foundational (T010) — can proceed in parallel with each other and with US1 after checkpoint
- **US5 (Phase 7)**: Depends on Foundational (T003, T004, T008) and T013/T030 for yield wiring
- **Polish (Phase 8)**: Depends on all phases complete

### User Story Dependencies

- **US1**: No inter-story dependencies. Start immediately after Foundational.
- **US2, US3, US4**: All independent from each other. Can run in parallel after Foundational.
- **US5**: T030 uses T027 (device generation) and T012 (scan command); T031 uses T013 (inject command). Otherwise independent.

### Within Each User Story

- T011–T016 (command overrides) are mutually parallel — different files
- T017 (CommandParser accessor) is parallel with T011–T016
- T018 (TerminalController) depends on T011–T017
- T019 (RegisterCommands) depends on T009 and T011–T016
- T020 (tests) depends on T006 only — can be written before T011–T019

---

## Parallel Execution Examples

### Foundational Phase (all can start simultaneously)

```
T001 HardwareStat enum
T002 Player model fields
T003 Device model fields
T004 DeviceSaveData + Version=3
T007 ICommand.GetLatency default
```
Then: T005 (needs T001), T006 (needs T002, T003), T008 (needs T002, T004), T010 (needs T001, T002, T005)
Then: T009 (needs T006, T008)

### US1 Command Overrides (all parallel after Foundational)

```
T011 CrackCommand.GetLatency
T012 ScanCommand.GetLatency
T013 InjectCommand.GetLatency
T014 FirewallCommand.GetLatency
T015 LsCommand.GetLatency
T016 CopyCommand.GetLatency
T017 CommandParser.GetCommand
T020 CommandLatencyServiceTests
```

### US2 + US3 + US4 (all parallel after Foundational)

```
T021 CPU shop assets
T023 Bandwidth shop assets
T025 GPU shop assets
```

### US5 Location + Save (parallel after Foundational)

```
T027 GenerateLocation device tiers
T028 ToSaveData device tiers
T029 RestoreFromSave device tiers
```

---

## Implementation Strategy

### MVP (User Story 1 Only)

1. Complete Phase 2 (Foundational)
2. Complete Phase 3 (US1)
3. **Validate**: All commands show variable-length bars; `show`/`help`/`move`/`forget` instant; progress bar duration matches hardware tier
4. Stop and demo if needed

### Incremental Delivery

1. Foundational → US1 (MVP: variable command timing)
2. + US2 + US3 + US4 (all three shop upgrade axes) — can be done in one session
3. + US5 (target device tiers, payload yield) — deepens strategy without breaking prior stories
4. Polish (Phase 8) before PR merge

---

## Notes

- `[P]` = different files, no inter-task dependency; safe to run in parallel
- `[USn]` = maps to User Story n in `spec.md` for traceability
- `plan.md`, `data-model.md`, `contracts/command-schema.md` contain the **old resistance model** — do not use their formulas; use the formulas in T006 and T013/T014 descriptions above
- `BandwidthSpeedupTable[0]` (tier 1) = `1.0f` → satisfies FR-021: tier-1 target = neutral baseline (no overhead, no extra benefit)
- Maximum speedup for network commands = 4.0× (BW tier 5); maximum for crack = 20.0× (CPU5 × GPU3) — SC-001's "20×" applies specifically to crack and to the combined system ceiling, not to every individual command (per Decision 3 in research.md, which remains valid)
- T031 (payload yield) scope: locate the existing income/miner calculation for `inject` and substitute `device.CpuTier` as the yield multiplier — if no yield system exists yet, add a `yieldTier` field to the running payload record as a foundation for a future feature
