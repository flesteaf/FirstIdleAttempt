# Implementation Plan: Network Discovery Income — Direct Inject by IP/SSID

**Branch**: `feature/002-network-discovery-income`
**Date**: 2026-04-13
**Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/002-network-discovery-income/spec.md`

## Summary

Extends `InjectCommand` to accept two optional trailing arguments — `{IP}` and `{SSID}`.
When both are supplied the command resolves the target device directly from
`LocationService.GetCurrentLocation()` rather than from `Player.TargetedDevice`,
then runs identical precondition checks and injection logic as the existing path.
When neither argument is present the command is fully backward compatible.

Three source files change (`InjectCommand.cs`, `GameManager.cs`,
`InjectCommandTests.cs`). One word changes in a fourth (`LocationService.cs`
— add `virtual` to `GetCurrentLocation()`). The command schema is amended to v1.1.0.

## Technical Context

**Language/Version**: C# 9 (.NET Standard 2.1, Unity 6 scripting runtime)
**Primary Dependencies**: Unity 6 (6000.4.2f1), Unity Test Framework (NUnit)
**Storage**: N/A — no persistence changes
**Testing**: Unity Test Framework — edit-mode unit tests
**Target Platform**: PC (Windows), desktop, keyboard input
**Project Type**: 2D idle/incremental desktop game
**Performance Goals**: No new hot paths introduced
**Constraints**: No LINQ in for-loops; no new GC allocations in command path
**Scale/Scope**: Changes confined to 4 existing files; no new files

**Changed files**:
- `Assets/Scripts/Services/Commands/InjectCommand.cs` — add LocationService param + arg routing
- `Assets/Scripts/Services/LocationService.cs` — add `virtual` to `GetCurrentLocation()` (1 word)
- `Assets/Scripts/Core/GameManager.cs` — pass `LocationService` to `InjectCommand` ctor (1 line)
- `Assets/Tests/EditMode/InjectCommandTests.cs` — append 9 new test cases + stub class

No new files required. No new models, services, ScriptableObjects, or UI components.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status | Notes |
|-----------|------|--------|-------|
| I. Code Quality | SRP; no magic numbers; XML docs on all public APIs | ✅ PASS | `InjectCommand` gains one private `TryResolveDevice` helper. SRP maintained — class still only handles inject logic. New string literals are error messages, not magic numbers. XML docs added on new constructor param and helper. |
| II. Testing Standards | TDD; tests FAILING before implementation; edit-mode coverage | ✅ PASS | `tasks.md` mandates test-first. 9 new test cases appended to existing test file. |
| III. UX Consistency | Commands follow command-schema.md exactly; explicit feedback on every path | ✅ PASS | Schema bumped to v1.1.0 (non-breaking). All new error messages defined in schema before implementation begins. |
| IV. Performance | No per-frame Update(); no LINQ in hot paths; no GC allocations | ✅ PASS | Device resolution is two `for` loops over small lists (2–5 networks, 1–4 devices). No LINQ. Not in any tick path. |

**Post-design re-check**: All gates pass. No violations.

## Architecture Decisions

**Why add `LocationService` to `InjectCommand` rather than resolving in `GameManager`?**
Each command owns its own argument parsing and precondition validation — SRP as established
by `ScanCommand`, `CrackCommand`, `ShowCommand` which all take `LocationService`.
Placing resolution logic in `GameManager.RegisterCommands` would scatter command logic
across two files.

**Why NOT update `Player.TargetedDevice` in the direct-inject path?**
The direct-inject path is an injection shortcut only. Silently updating session targeting
as a side effect would create confusing implicit state visible to subsequent `firewall`
and `ls` commands. Targeting remains the exclusive responsibility of `ScanCommand`.

**Why restrict to current location only?**
Cross-location injection would require searching the full location cache and creates
ambiguity if the same IP appears in multiple procedurally-generated locations (the
generator does not guarantee IP uniqueness across locations). Current location is
unambiguous and matches the player's mental model.

**Why `virtual` on `GetCurrentLocation()` rather than an `ILocationService` interface?**
The existing codebase has no `IService` interfaces for services — they are constructed
directly. Introducing an interface for one test doubles the scope. Making one method
`virtual` is the minimal, consistent change.

## Project Structure

### Documentation (this feature)

```text
specs/002-network-discovery-income/
├── plan.md                        # This file
├── spec.md                        # Feature specification
├── contracts/
│   └── command-schema.md          # inject command schema v1.1.0
├── tasks.md                       # TDD task list
└── specify-prompts.md             # Speckit prompt texts for regeneration
```

### Source Code (changes only)

```text
Assets/
└── Scripts/
    ├── Services/
    │   ├── Commands/
    │   │   └── InjectCommand.cs        ← modified: LocationService ctor + arg routing
    │   └── LocationService.cs          ← modified: add virtual to GetCurrentLocation()
    └── Core/
        └── GameManager.cs              ← modified: pass LocationService to InjectCommand
Assets/
└── Tests/
    └── EditMode/
        └── InjectCommandTests.cs       ← modified: append 9 new tests + StubLocationService
```

**Structure Decision**: All changes are modifications to existing files only. No new
files, folders, assemblies, or ScriptableObjects are needed.

## Complexity Tracking

> No constitution violations — section not required.
