# Implementation Plan: Game Save Management

**Branch**: `007-game-save-management` | **Date**: 2026-04-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/007-game-save-management/spec.md`

## Summary

Replace the single-file `save.json` system with a 7-slot save system exposed via five new terminal commands (`save`, `load`, `delsave`, `newgame`, `saves`). Each slot is stored as an independent `save_{N}.json` file; a lightweight `save_index.json` tracks occupancy and timestamps. Overwrite and new-game resets both require explicit in-terminal confirmation via a new `ConfirmationService`. `GameManager` gains `LoadSlot(int)` and `NewGame()` methods that re-bootstrap all services from the new slot data without a scene reload. Settings management (audio, colour scheme, difficulty) is out of scope.

## Technical Context

**Language/Version**: C# 9 (.NET Standard 2.1, Unity 6 scripting runtime)  
**Primary Dependencies**: Unity 6 (6000.4.2f1), Unity Test Framework (NUnit)  
**Storage**: JSON flat files via `JsonUtility` at `Application.persistentDataPath/save_{N}.json` (slots 1–7) and `save_index.json` (slot metadata index)  
**Testing**: NUnit edit-mode tests via Unity Test Framework  
**Target Platform**: PC (Windows, integrated GPU, 4 GB RAM minimum)  
**Project Type**: Desktop game (Unity 6, 2D URP)  
**Performance Goals**: All save/load operations complete in under 3 seconds; no file I/O in `Update()` or tick paths  
**Constraints**: `JsonUtility` compatibility (no `Dictionary`, no top-level `List<>` serialisation); pure C# for all new service classes; all public APIs require XML doc comments; SRP per class; dead code must not be committed  

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design — all gates still pass.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Code Quality | ✅ PASS | Each command class has one responsibility. `SlotSaveSystem` owns file-slot I/O only. `ConfirmationService` owns pending-confirmation state only. All new public APIs have XML doc comments. Slot range `[1, 7]` stored as `const int MaxSlots = 7` — no magic numbers. |
| II. Testing Standards | ✅ PASS (with obligation) | 7 new test files required before implementation begins (Red-Green-Refactor). Tests cover: slot round-trip, index read/write, legacy migration, each command's success/error/confirmation paths, `ConfirmationService` state transitions. |
| III. UX Consistency | ✅ PASS | Command output matches existing plain-text style (`Ok`/`Error:` format). Confirmation prompts end with `(y/n)` — consistent and unambiguous. `saves` uses 7-line fixed format consistent with `ls` output width. |
| IV. Performance | ✅ PASS | File I/O only on explicit player command — never in `Update()` or tick paths. In-session re-bootstrap is a one-time event. `saves` reads only the small index file, not 7 full saves. No heap allocations in hot paths. |

**Quality Gates pre-merge**:
1. **Build Gate**: Zero compiler errors/warnings in Unity 6 (6000.4.2f1).
2. **Test Gate**: All 7 new test files green; existing test suite must not regress.
3. **Performance Gate**: No new `Update()` hot paths. Profiler data not required (no new hot paths added).
4. **UX Gate**: All 5 commands output text consistent with existing command style; confirmation prompts are clear and resolve correctly.
5. **Constitution Check**: Reviewer confirms all four principles in PR checklist.

## Project Structure

### Documentation (this feature)

```text
specs/007-game-save-management/
├── plan.md                         # This file
├── research.md                     # Phase 0 — 10 decisions
├── data-model.md                   # Phase 1 — 6 entities
├── quickstart.md                   # Phase 1 — developer guide
├── contracts/
│   └── command-schema.md           # Phase 1 — per-command I/O contract
└── tasks.md                        # Phase 2 (/speckit.tasks output)
```

### Source Code Layout

```text
Assets/Scripts/
├── Core/
│   ├── GameManager.cs              # MODIFY: add CurrentSlot, LoadSlot(), NewGame(), SlotSaveSystem, ConfirmationService; guard PersistSession() on CurrentSlot == -1; call MigrateLegacyIfNeeded()
│   ├── SaveSystem.cs               # KEEP: unchanged (reused internally by SlotSaveSystem)
│   └── SlotSaveSystem.cs           # NEW: multi-slot save management (7 slots + save_index.json)
├── Models/
│   ├── SaveData.cs                 # KEEP: schema unchanged (Version stays at 3)
│   └── SaveSlotInfo.cs             # NEW: SaveSlotInfo + SaveSlotIndex (slot metadata models)
├── Services/
│   ├── ConfirmationService.cs      # NEW: pending yes/no confirmation state machine
│   └── Commands/
│       ├── SaveCommand.cs          # NEW: 'save <slot>'
│       ├── LoadCommand.cs          # NEW: 'load <slot>'
│       ├── DelsaveCommand.cs       # NEW: 'delsave <slot>'
│       ├── NewGameCommand.cs       # NEW: 'newgame'
│       └── SavesCommand.cs         # NEW: 'saves'
└── UI/
    └── TerminalController.cs       # MODIFY: route input through ConfirmationService.IsPending before CommandParser

Assets/Tests/EditMode/
├── SlotSaveSystemTests.cs          # NEW: save/load round-trip, index, delete, migration
├── ConfirmationServiceTests.cs     # NEW: state transitions (pending, resolve, cancel, replace)
├── SaveCommandTests.cs             # NEW: empty slot, occupied + confirm/decline, range errors
├── LoadCommandTests.cs             # NEW: occupied slot, empty slot, range errors
├── DelsaveCommandTests.cs          # NEW: occupied slot, empty slot, active-slot reset
├── NewGameCommandTests.cs          # NEW: confirm/decline, CurrentSlot reset, saves preserved
└── SavesCommandTests.cs            # NEW: all empty, mixed, all occupied; format verification
```
