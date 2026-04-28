# Tasks: Game Save Management

**Input**: Design documents from `/specs/007-game-save-management/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/command-schema.md ✓, quickstart.md ✓

**Tests**: Required per constitution (plan.md) — 7 new test files must be written RED before implementation GREEN. TDD: Red-Green-Refactor is mandatory, not optional.

**Organization**: Tasks grouped by user story. Phase 2 (Foundational) blocks all user stories. US phases are independent once foundational is complete.

**Total tasks**: 27 across 8 phases (1 setup · 7 foundational · 3+4+3+4+3 per US · 2 polish)

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1–US5)
- Exact file paths required in every task description

---

## Phase 1: Setup (Shared Models)

**Purpose**: Create the data model types that all components depend on. No prerequisites — start immediately.

- [x] T001 Create `SaveSlotInfo` and `SaveSlotIndex` classes (`[System.Serializable]`, `JsonUtility`-compatible, XML doc comments on all public members) in `Assets/Scripts/Models/SaveSlotInfo.cs` per data-model.md Entities 1 and 2 (`SaveSlotInfo`: `SlotNumber int`, `IsOccupied bool`, `SavedAtUtcTicks long`; `SaveSlotIndex`: `Slots SaveSlotInfo[]` length=7, `Slots[i].SlotNumber == i+1` invariant)

**Checkpoint**: Model types available — foundational phase can begin.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core services and GameManager wiring that ALL user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### API Stubs (required before test compilation)

- [x] T00X [P] Create `SlotSaveSystem` public API stub — all 6 public method signatures throwing `NotImplementedException`, with XML doc comments: `LoadSlot(int slot) : SaveData`, `SaveSlot(int slot, SaveData data)`, `DeleteSlot(int slot)`, `LoadIndex() : SaveSlotIndex`, `SaveIndex(SaveSlotIndex index)`, `MigrateLegacyIfNeeded()`; constructor accepts `string basePath` for test isolation — in `Assets/Scripts/Core/SlotSaveSystem.cs`
- [x] T00X [P] Create `ConfirmationService` public API stub — all public members returning defaults/no-ops with XML doc comments: `IsPending bool` property, `PendingPrompt string` property, `RequestConfirmation(string prompt, Action onConfirm, Action onCancel)`, `Resolve(bool confirmed)`, `Cancel()` — in `Assets/Scripts/Services/ConfirmationService.cs`

### Red Phase — Failing Tests (write BEFORE implementation)

- [x] T00X Write `SlotSaveSystemTests`: save/load round-trip (slot 1–7), index reflects occupancy+timestamp after save, delete clears file and marks index empty, `MigrateLegacyIfNeeded` copies `save.json`→`save_1.json` when no slot files exist, slot 0 and slot 8 throw `ArgumentOutOfRangeException`; use `Path.GetTempPath()` subdirectory isolation per quickstart.md — in `Assets/Tests/EditMode/SlotSaveSystemTests.cs` (must compile and FAIL against T002 stub)
- [x] T00X [P] Write `ConfirmationServiceTests`: `IsPending` starts false; `RequestConfirmation` sets `IsPending=true` and stores prompt; `Resolve(true)` fires `onConfirm` and clears pending; `Resolve(false)` fires `onCancel` and clears pending; `Cancel()` clears without firing callbacks; second `RequestConfirmation` while pending replaces first (no `onCancel` fired for replaced) — in `Assets/Tests/EditMode/ConfirmationServiceTests.cs` (must compile and FAIL against T003 stub)

### Green Phase — Implementations

- [x] T00X Implement `SlotSaveSystem`: path helpers `{basePath}/save_{slot}.json` and `{basePath}/save_index.json`; validate slot in [1,7] throwing `ArgumentOutOfRangeException`; `SaveSlot` writes `save_{slot}.json` via `JsonUtility.ToJson` then updates `SaveIndex`; `LoadSlot` reads file via `JsonUtility.FromJson`, returns null if absent; `DeleteSlot` deletes file and marks index entry empty (`IsOccupied=false`, `SavedAtUtcTicks=0`); `LoadIndex` returns fresh 7-entry default when file absent; `MigrateLegacyIfNeeded` checks `save.json` exists and no `save_1.json`–`save_7.json` exist, then copies to `save_1.json` and writes index with slot 1 occupied — in `Assets/Scripts/Core/SlotSaveSystem.cs` (all T004 tests must pass)
- [x] T00X [P] Implement `ConfirmationService`: inner `PendingConfirmation` record holding `Prompt`, `OnConfirm`, `OnCancel`; `RequestConfirmation` stores new record (replacing any existing); `Resolve(true)` invokes `OnConfirm` then nulls record; `Resolve(false)` invokes `OnCancel` then nulls record; `Cancel` nulls record without invoking callbacks — in `Assets/Scripts/Services/ConfirmationService.cs` (all T005 tests must pass)
- [x] T00X Modify `GameManager`: add `CurrentSlot int` property (default -1); add `SlotSaveSystem SlotSaveSystem` field constructed with `Application.persistentDataPath`; add `ConfirmationService ConfirmationService` property; guard `PersistSession()` to no-op when `CurrentSlot == -1`; call `SlotSaveSystem.MigrateLegacyIfNeeded()` from `Bootstrap()`; if migration ran, set `CurrentSlot = 1` — in `Assets/Scripts/Core/GameManager.cs`

**Checkpoint**: Foundation complete — all 5 user story phases can now begin (independently, in priority order).

---

## Phase 3: User Story 1 — Save Game Progress (Priority: P1) 🎯 MVP

**Goal**: Player saves current progress to any numbered slot (1–7). Occupied slots require an explicit overwrite confirmation.

**Independent Test**: Issue `save 3` to empty slot → `save_3.json` exists, index shows slot 3 occupied. Issue `save 3` again → confirm "y" → file updated. Issue `save 3` → confirm "n" → file unchanged. Issue `save 8` → error message returned.

### Red Phase

- [x] T00X [US1] Write `SaveCommandTests`: empty slot → returns `"Game saved to slot N."`; occupied slot → returns `"Slot N is occupied, overwrite? (y/n)"`; `onConfirm` callback saves and returns `"Game saved to slot N."`; `onCancel` callback returns `"Save cancelled."`; slot 0/8 → `"Error: Slot must be between 1 and 7."`; missing arg → `"Error: Usage: save <slot>"`; non-integer arg → `"Error: Slot must be between 1 and 7."` — in `Assets/Tests/EditMode/SaveCommandTests.cs` (must compile and FAIL)

### Green Phase

- [x] T0XX [US1] Implement `SaveCommand`: constructor takes `SlotSaveSystem`, `ConfirmationService`, `Func<SaveData> getCurrentData`, `Action<int> setActiveSlot`; `Execute(string[] args)` parses `args[0]` with `TryParseSlot()`, validates [1,7], checks `LoadIndex().Slots[slot-1].IsOccupied`; on occupied calls `_confirmationService.RequestConfirmation(prompt, onConfirm, onCancel)` and returns prompt as `Ok`; on empty writes slot directly and returns success; `onConfirm` calls `SaveSlot` then `setActiveSlot(slot)` — in `Assets/Scripts/Services/Commands/SaveCommand.cs` (all T009 tests must pass)
- [x] T0XX [US1] Register `SaveCommand` with verb `"save"` in `GameManager.RegisterCommands()`, injecting `SlotSaveSystem`, `ConfirmationService`, `() => SaveData`, and `slot => CurrentSlot = slot` callback — in `Assets/Scripts/Core/GameManager.cs`

**Checkpoint**: US1 complete. Player can save to any slot with overwrite protection.

---

## Phase 4: User Story 2 — Load a Saved Game (Priority: P2)

**Goal**: Player restores a previously saved state from any occupied slot. Triggers full in-session re-bootstrap (no scene reload).

**Independent Test**: Save to slot 2, modify state, issue `load 2` → state matches saved snapshot, `CurrentSlot == 2`. Issue `load 6` (empty) → error returned. Issue `load 9` → range error.

### Red Phase

- [x] T0XX [US2] Write `LoadCommandTests`: occupied slot → calls `loadSlot` callback with correct slot number and returns `"Game loaded from slot N."`; empty slot → `"Error: Slot N is empty."`; out-of-range → `"Error: Slot must be between 1 and 7."`; missing arg → `"Error: Usage: load <slot>"`; non-integer arg → `"Error: Slot must be between 1 and 7."` — in `Assets/Tests/EditMode/LoadCommandTests.cs` (must compile and FAIL)

### Green Phase

- [x] T0XX [US2] Add `GameManager.LoadSlot(int slot)`: call `SlotSaveSystem.LoadSlot(slot)`, apply `MigrateIfNeeded` to loaded data, replace `SaveData`, re-initialise `LocationService` and `CommandLatencyService` from new data, call `RegisterCommands()` to replace all stale command instances, set `CurrentSlot = slot` — in `Assets/Scripts/Core/GameManager.cs`
- [x] T0XX [US2] Implement `LoadCommand`: constructor takes `SlotSaveSystem`, `Action<int> loadSlot`; `Execute(string[] args)` parses slot, validates range, checks occupancy via `LoadIndex()`; on empty returns error; on occupied calls `_loadSlot(slot)` and returns success — in `Assets/Scripts/Services/Commands/LoadCommand.cs` (all T012 tests must pass)
- [x] T0XX [US2] Register `LoadCommand` with verb `"load"` in `GameManager.RegisterCommands()`, injecting `SlotSaveSystem` and `slot => LoadSlot(slot)` callback — in `Assets/Scripts/Core/GameManager.cs`

**Checkpoint**: US1 + US2 complete. Full save/load cycle operational.

---

## Phase 5: User Story 3 — Remove a Saved Game (Priority: P3)

**Goal**: Player deletes a save slot, marking it empty. Deleting the currently active slot resets `CurrentSlot` to -1.

**Independent Test**: Save to slot 4, issue `delsave 4` → `save_4.json` absent, `saves` shows slot 4 `[empty]`. Issue `delsave 1` (empty) → error. Issue `delsave N` where N == `CurrentSlot` → `CurrentSlot` becomes -1.

### Red Phase

- [x] T0XX [US3] Write `DelsaveCommandTests`: occupied slot → calls `deleteSlot` callback and returns `"Save slot N removed."`; empty slot → `"Error: Slot N has no save to remove."`; out-of-range → `"Error: Slot must be between 1 and 7."`; missing arg → `"Error: Usage: delsave <slot>"`; deleting active slot fires `resetCurrentSlotIfActive` callback — in `Assets/Tests/EditMode/DelsaveCommandTests.cs` (must compile and FAIL)

### Green Phase

- [x] T0XX [US3] Implement `DelsaveCommand`: constructor takes `SlotSaveSystem`, `Action<int> resetCurrentSlotIfActive`; `Execute(string[] args)` parses slot, validates range, checks occupancy; on empty returns error; on occupied calls `_slotSaveSystem.DeleteSlot(slot)`, invokes `_resetCurrentSlotIfActive(slot)`, returns success — in `Assets/Scripts/Services/Commands/DelsaveCommand.cs` (all T016 tests must pass)
- [x] T0XX [US3] Register `DelsaveCommand` with verb `"delsave"` in `GameManager.RegisterCommands()`, injecting `SlotSaveSystem` and `slot => { if (CurrentSlot == slot) CurrentSlot = -1; }` callback — in `Assets/Scripts/Core/GameManager.cs`

**Checkpoint**: US1–US3 complete. Full slot lifecycle (save/load/delete) operational.

---

## Phase 6: User Story 4 — Start a New Game (Priority: P4)

**Goal**: Player resets to the default initial state. Always prompts for confirmation. Existing saves are never touched.

**Independent Test**: Issue `newgame` → prompt displayed. Confirm "y" → `SaveData` matches `CreateDefault()`, `CurrentSlot == -1`, all slot files unchanged. Decline "n" → state unchanged.

### Red Phase

- [x] T0XX [US4] Write `NewGameCommandTests`: command always returns `"Unsaved progress will be lost. Continue? (y/n)"`; `onConfirm` fires `newGame` callback and returns `"New game started."`; `onCancel` returns `"New game cancelled."`; confirms `CurrentSlot` set to -1 via callback; slot files not modified — in `Assets/Tests/EditMode/NewGameCommandTests.cs` (must compile and FAIL)

### Green Phase

- [x] T0XX [US4] Add `GameManager.NewGame()`: replace `SaveData` with `SaveData.CreateDefault()`, re-initialise `LocationService` and `CommandLatencyService`, call `RegisterCommands()` to replace stale instances, set `CurrentSlot = -1` — in `Assets/Scripts/Core/GameManager.cs`
- [x] T0XX [US4] Implement `NewGameCommand`: constructor takes `ConfirmationService`, `Action newGame`; `Execute(string[] args)` calls `_confirmationService.RequestConfirmation("Unsaved progress will be lost. Continue? (y/n)", onConfirm, onCancel)`; `onConfirm` calls `_newGame()` and routes `"New game started."` result; `onCancel` returns `"New game cancelled."` — in `Assets/Scripts/Services/Commands/NewGameCommand.cs` (all T019 tests must pass)
- [x] T0XX [US4] Register `NewGameCommand` with verb `"newgame"` in `GameManager.RegisterCommands()`, injecting `ConfirmationService` and `NewGame` callback — in `Assets/Scripts/Core/GameManager.cs`

**Checkpoint**: US1–US4 complete. Save/load/delete/new-game lifecycle fully operational.

---

## Phase 7: User Story 5 — View Save Slot Overview (Priority: P5)

**Goal**: Player sees all 7 slots at a glance — occupied slots show UTC timestamp, empty slots show `[empty]`.

**Independent Test**: Issue `saves` with slots 1+3 occupied → 7-line output; lines 1 and 3 show `yyyy-MM-dd HH:mm`; lines 2,4–7 show `[empty]`. Issue `saves` all-empty → all 7 lines show `[empty]`, no errors.

### Red Phase

- [x] T0XX [US5] Write `SavesCommandTests`: all-empty → 7 lines each matching `"Slot N: [empty]"`; occupied slot → line matches `"Slot N: yyyy-MM-dd HH:mm"` using `new DateTime(ticks, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm")`; all-occupied → 7 timestamp lines; no errors thrown in any state — in `Assets/Tests/EditMode/SavesCommandTests.cs` (must compile and FAIL)

### Green Phase

- [x] T0XX [US5] Implement `SavesCommand`: constructor takes `SlotSaveSystem`; `Execute(string[] args)` calls `LoadIndex()`, iterates all 7 `Slots` entries, formats each as `"Slot N: yyyy-MM-dd HH:mm"` (UTC, `DateTimeKind.Utc`) when occupied or `"Slot N: [empty]"` when not, returns joined multi-line `CommandResult.Ok` — in `Assets/Scripts/Services/Commands/SavesCommand.cs` (all T023 tests must pass)
- [x] T0XX [US5] Register `SavesCommand` with verb `"saves"` in `GameManager.RegisterCommands()`, injecting `SlotSaveSystem` — in `Assets/Scripts/Core/GameManager.cs`

**Checkpoint**: All 5 user stories complete. Save management system fully functional.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Terminal UI wiring and final quality-gate compliance.

- [x] T0XX Modify `TerminalController` input handler: before routing to `CommandParser`, check `GameManager.Instance.ConfirmationService.IsPending`; if pending, call `ConfirmationService.Resolve(input.Trim().ToLower() == "y")` and return (do NOT re-route to parser); implement "command-while-pending = cancel" rule: if input is not `"y"` or `"n"`, call `Resolve(false)` and discard input (per command-schema.md routing contract) — in `Assets/Scripts/UI/TerminalController.cs`
- [x] T0XX [P] Add XML doc comments to all remaining public APIs lacking them: `SaveSlotInfo`, `SaveSlotIndex` fields; `SaveCommand`, `LoadCommand`, `DelsaveCommand`, `NewGameCommand`, `SavesCommand` constructors and `Execute()` methods (plan.md constitution requirement I: all public APIs require XML doc comments) — across `Assets/Scripts/`

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 1 (Setup: T001)
  └─► Phase 2 (Foundational: T002–T008)  ← BLOCKS all user stories
        ├─► Phase 3 (US1 P1: T009–T011) ─►
        ├─► Phase 4 (US2 P2: T012–T015) ─► Phase 8 (Polish: T026–T027)
        ├─► Phase 5 (US3 P3: T016–T018) ─►
        ├─► Phase 6 (US4 P4: T019–T022) ─►
        └─► Phase 7 (US5 P5: T023–T025) ─►
```

### User Story Dependencies

- **US1 (P1)**: Phase 2 only. No story dependencies.
- **US2 (P2)**: Phase 2 only. No story dependencies. (Shares `SlotSaveSystem` already wired.)
- **US3 (P3)**: Phase 2 only. No story dependencies.
- **US4 (P4)**: Phase 2 only. No story dependencies.
- **US5 (P5)**: Phase 2 only. No story dependencies.

### Within Phase 2 (internal sequencing)

```
T002 (SlotSaveSystem stub) ─► T004 (SlotSaveSystemTests RED) ─► T006 (implement GREEN)
T003 (ConfirmationService stub) ─► T005 (ConfirmationServiceTests RED) ─► T007 (implement GREEN)
                        (both tracks fully parallel)
                                ↓ (both T006+T007 must complete)
                              T008 (GameManager wiring)
```

### ⚠️ Shared File: `GameManager.cs`

T008, T011, T013, T015, T018, T020, T022, T025 all modify `GameManager.cs`. These tasks cannot run concurrently. Single-developer workflow handles this naturally. Parallel-team workflow: coordinate GameManager changes at phase boundaries to avoid merge conflicts.

---

## Parallel Opportunities

### Phase 2 — Two independent tracks

```
Track A: T002 → T004 → T006     (SlotSaveSystem stub → tests → impl)
Track B: T003 → T005 → T007     (ConfirmationService stub → tests → impl)
Both tracks merge at T008 (GameManager wiring)
```

### Post-Phase-2 — Five independent user story tracks

```
Track 1: T009 → T010 → T011   (US1 SaveCommand)
Track 2: T012 → T013 → T014 → T015   (US2 LoadCommand + LoadSlot)
Track 3: T016 → T017 → T018   (US3 DelsaveCommand)
Track 4: T019 → T020 → T021 → T022   (US4 NewGameCommand + NewGame)
Track 5: T023 → T024 → T025   (US5 SavesCommand)
```

*(Coordinate on `GameManager.cs` RegisterCommands additions — merge after each story's final task)*

---

## Implementation Strategy

### MVP Scope (User Story 1 only)

1. T001 — Setup
2. T002–T008 — Foundational (**mandatory before anything else**)
3. T009–T011 — US1 SaveCommand
4. **Validate**: `save 3` on empty slot; `save 3` again (confirm overwrite); `save 8` (range error)
5. Demo: player can save progress to a slot

### Incremental Delivery

| Phase done | What player can do |
|---|---|
| Phase 2 | Nothing visible yet — foundation only |
| Phase 3 (US1) | `save <slot>` — save with overwrite protection |
| Phase 4 (US2) | `load <slot>` — restore saved state |
| Phase 5 (US3) | `delsave <slot>` — free up slots |
| Phase 6 (US4) | `newgame` — reset with confirmation |
| Phase 7 (US5) | `saves` — overview of all 7 slots |
| Phase 8 | All commands work end-to-end in terminal UI |

---

## Notes

- **TDD is mandatory** per plan.md constitution — all 7 test files must be RED before their paired implementation begins
- `SlotSaveSystem` takes a `string basePath` constructor arg for test isolation; never hardcode `Application.persistentDataPath` inside the class — pass it at construction time
- `JsonUtility` constraint: no `Dictionary`, no top-level `List<>` serialisation — `SaveSlotIndex` wrapper class is the fix
- All output strings must match `contracts/command-schema.md` verbatim (exact casing, punctuation, spacing)
- `CurrentSlot = -1` means "no active slot"; `PersistSession()` must be a no-op in this state (prevents overwriting an existing slot after `newgame`)
- Legacy `save.json` is **not deleted** after migration — left in place as backup per research.md Decision 6
- Dead code must not be committed — remove any method stubs once implementation is complete
