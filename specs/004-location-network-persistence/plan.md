# Implementation Plan: Location, Network Persistence & Movement

**Branch**: `feature/004-location-network-persistence` | **Date**: 2026-04-18 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `specs/004-location-network-persistence/spec.md`

## Summary

Fix `show networks`/`show ips`/`IncomeService` to aggregate across all known locations (not just current), add deterministic location naming (`node_77` formula), implement `move` and `forget` commands, introduce interactive arrow-key selection for `inject`/`firewall`/`ls`/`copy`, and remove implicit `TargetedDevice`/`TargetedNetwork` targeting. Save schema bumped to v2 with `Name` and `CurrentLocationName` fields plus backward-compatible migration.

## Technical Context

**Language/Version**: C# 9 (.NET Standard 2.1, Unity 6 scripting runtime)  
**Primary Dependencies**: Unity 6 (6000.4.2f1), TextMeshPro, Unity Input System, Unity Test Framework (NUnit)  
**Storage**: JSON flat file via `JsonUtility` to `Application.persistentDataPath/save.json`  
**Testing**: Unity Test Framework (NUnit) — EditMode tests at `Assets/Tests/EditMode/`, PlayMode at `Assets/Tests/PlayMode/`  
**Target Platform**: Desktop (Windows/Mac, Unity Player)  
**Project Type**: Desktop game (Unity MonoBehaviour architecture)  
**Performance Goals**: 45 FPS; `IncomeService.OnTick()` < 1 ms per frame; no GC allocations in tick hot path  
**Constraints**: `ICommand.Execute()` is synchronous; `CommandResult` has only `bool Success` + `string Message`; Unity `JsonUtility` serialises only public fields; `[System.NonSerialized]` fields are not persisted  
**Scale/Scope**: ~10–50 locations per session; < 100 devices total; single-player; no networked state

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| §I SRP — each class has one responsibility | PASS | New `DeviceSelector` (Commands/), `ForgetResult` (Models/), `MoveCommand`, `ForgetCommand` each have single clear purpose |
| §I XML docs on public API | ACTION REQUIRED | Task G-4 mandates XML docs on all new `LocationService` public methods |
| §I No magic numbers | ACTION REQUIRED | `77`, `1000` in name formula must be named constants |
| §II Red-Green-Refactor; no coverage regression | PASS | Every task specifies unit tests; A-5 adds profiler validation |
| §III UX consistency (command format, playtesting) | ACTION REQUIRED | Task G-3 is the manual UX gate — must be completed before PR merge |
| §IV 45 FPS / 22.22 ms budget; no hot-path alloc | PASS | `GetAllKnownLocations()` returns `_locationCache.Values` (no allocation); A-5 validates in Profiler |

**Post-design re-check**: No constitution violations introduced by Phase 1 design. Named constants will be introduced during implementation of B-2.

## Project Structure

### Documentation (this feature)

```text
specs/004-location-network-persistence/
├── plan.md              # This file
├── research.md          # Phase 0 — 10 architectural decisions
├── data-model.md        # Phase 1 — entity definitions and state transitions
├── contracts/
│   └── command-schema.md  # Phase 1 — all command verb contracts
└── tasks.md             # Task Groups A–J (already created)
```

### Source Code (repository root)

```text
Assets/Scripts/
├── Core/
│   └── GameManager.cs               # Register move, forget; v1→v2 migration
├── Models/
│   ├── Location.cs                  # + Name field
│   ├── SaveData.cs                  # + CurrentLocationName, bump Version constant
│   └── ForgetResult.cs              # new readonly struct
├── Services/
│   ├── LocationService.cs           # GetAllKnownLocations(), SetCurrentLocation(),
│   │                                #   GetCurrentLocationName(), ForgetNetwork(),
│   │                                #   ForgetDevice(), _knownLocationsSnapshot
│   ├── IncomeService.cs             # OnTick() → GetAllKnownLocations()
│   └── Commands/
│       ├── MoveCommand.cs           # new
│       ├── ForgetCommand.cs         # new
│       ├── DeviceSelector.cs        # new static helper
│       ├── ShowCommand.cs           # networks, ips, locations sub-commands
│       ├── ScanCommand.cs           # remove double-scan; add no-location error
│       ├── InjectCommand.cs         # interactive type + network selection
│       ├── FirewallCommand.cs       # optional IP arg; interactive fallback
│       ├── LsCommand.cs             # optional IP arg; interactive fallback
│       ├── CopyCommand.cs           # optional IP arg; interactive fallback
│       └── HelpCommand.cs           # move, forget, show locations, updated verb entries
└── UI/
    ├── TerminalController.cs        # AwaitSelection() selection mode
    └── HUDController.cs             # _locationLabel TextMeshProUGUI

Assets/Tests/EditMode/
├── LocationServiceTests.cs          # A-0..A-4, B-1..B-3, C-1..C-2, D-1..D-2, F-1..F-3
├── ShowCommandTests.cs              # A-2, A-3, E-1, E-2
├── MoveCommandTests.cs              # C-3
├── ForgetCommandTests.cs            # D-3
├── ScanCommandTests.cs              # I-1, I-2, I-3
├── InjectCommandTests.cs            # H-3, H-4, H-5
├── FirewallCommandTests.cs          # J-2
├── LsCommandTests.cs                # J-3
├── CopyCommandTests.cs              # J-4
├── TerminalControllerTests.cs       # H-1
└── DeviceSelectorTests.cs           # H-2
```

**Structure Decision**: Single Unity project. New files in existing `Commands/` and `Models/` directories; no new top-level directories required. `contracts/` and `data-model.md` are spec-only artifacts.

## Implementation Phases

### Phase 1 — Foundation (no UI, unblocked)

Tasks that carry no dependency on other groups and can be implemented in any order:

- **B-1**: Add `Name` to `Location` and `LocationSaveData`
- **F-1**: Add `CurrentLocationName` to `SaveData`
- **J-1**: Remove `TargetedDevice`/`TargetedNetwork` from `Player` (fix all compile errors)
- **D-1** (ForgetResult struct only): Create `Assets/Scripts/Models/ForgetResult.cs`

### Phase 2 — Core Service Layer

Depends on Phase 1:

- **A-0**: Verify `FromSaveData()` loads all locations
- **A-1**: `GetAllKnownLocations()` → `_locationCache.Values`
- **B-2**: Name generation formula in `GenerateLocation(seed)`
- **B-3**: Name restore/migrate in `FromSaveData()`
- **C-1**: `SetCurrentLocation(string name)`
- **C-2**: `GetCurrentLocationName()`
- **D-1** (service methods): `ForgetNetwork()`, `ForgetDevice()` in `LocationService`
- **F-2**: Write/read `CurrentLocationName` in `LocationService`
- **F-3**: v1→v2 migration in `GameManager`/`SaveSystem`

### Phase 3 — Income & Show Fix

Depends on Phase 2 (A-1 must be complete):

- **A-4**: `IncomeService.OnTick()` → `GetAllKnownLocations()`
- **A-2**: `show networks` aggregates all locations, column headers
- **A-3**: `show ips` aggregates all locations, column headers
- **E-1**: `show locations` sub-command
- **E-2**: Column headers for empty show output (validation tracking against FR-023/024)

### Phase 4 — New Commands

Depends on Phase 2:

- **C-3** + **C-4**: `MoveCommand` + register
- **D-3** + **D-4**: `ForgetCommand` + register
- **H-1**: `TerminalController.AwaitSelection()` selection mode

### Phase 5 — Interactive Commands

Depends on Phase 4 (H-1 must be complete):

- **H-2**: `DeviceSelector` helper
- **H-3**, **H-4**, **H-5**: `InjectCommand` interactive extensions
- **I-1**, **I-2**, **I-3**: `ScanCommand` behaviour changes
- **J-2**, **J-3**, **J-4**: `FirewallCommand`, `LsCommand`, `CopyCommand` optional IP + interactive

### Phase 6 — HUD, Help & Quality Gates

Depends on Phase 4 (location name available):

- **G-1**: `move` and `forget` help entries
- **G-2**: `show locations` help entry
- **H-6**: Updated `inject` help entry
- **J-5**: Updated `firewall`, `ls`, `copy` help entries
- **G-4**: XML documentation on new `LocationService` public methods
- **G-3**: Manual UX gate — terminal output samples in PR description
- **A-5**: Profiler validation of `IncomeService.OnTick()`

## Complexity Tracking

No constitution violations requiring justification. All design choices stay within existing architectural patterns (MonoBehaviour services, synchronous ICommand, JsonUtility save).
