# Implementation Plan: Hacking Idle Game — Core Game Loop

**Branch**: `develop` | **Date**: 2026-04-13 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/001-hacking-idle-core/spec.md`

## Summary

A single-player incremental/idle game with a hacking theme built in Unity 6
(6000.4.2f1) using URP 2D. The player uses terminal-style commands to discover
networks, crack security, and inject malware onto devices for passive income.
Progression is driven by a store offering PC component and software upgrades.
Idle income (miners, spammers) accrues indefinitely while the game is closed and
is applied on next launch. The economy starts with Bitcoin and unlocks altcoins
progressively via store milestones.

Architecture uses a timer-based `TickManager` (not per-frame Update) for income,
a `CommandParser` registry pattern, `ScriptableObject`-defined currencies and
store items, and JSON serialization for save data.

## Technical Context

**Language/Version**: C# 9 (.NET Standard 2.1, Unity 6 scripting runtime)
**Primary Dependencies**: Unity 6 (6000.4.2f1), URP 2D, TextMeshPro, Input System, Unity Test Framework (NUnit)
**Storage**: JSON flat file via `JsonUtility` to `Application.persistentDataPath/save.json` — note: `JsonUtility` does not serialize `Dictionary<K,V>`; use `List<CurrencyBalance>` wrapper instead (see data-model.md)
**Testing**: Unity Test Framework — edit-mode unit tests + play-mode integration tests
**Target Platform**: PC (Windows), desktop, keyboard input
**Project Type**: 2D idle/incremental desktop game
**Performance Goals**: 45 FPS minimum; <22.22 ms per frame; zero per-frame GC allocations in tick path
**Constraints**: No per-frame Update() for income accumulation; use `UnityEngine.Pool` (Unity 6 built-in) for UI element pooling; no LINQ in hot paths (IncomeService, CommandParser) — use `for` loops to avoid GC; use `StringBuilder` for terminal output building; `ProfilerRecorder` for automated frame-time assertions; Profiler data required on any new hot path PR
**Scale/Scope**: Single player; 10+ simultaneous infected devices; 3 contract types; progressive currency unlock

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status | Notes |
|-----------|------|--------|-------|
| I. Code Quality | Every system class MUST have one responsibility (SRP); no magic numbers | ✅ PASS | Design uses isolated classes per command, per service, per system |
| II. Testing Standards | Tests written before implementation; edit-mode + play-mode coverage | ✅ PASS | tasks.md will enforce TDD order; each system has a testable interface |
| III. UX Consistency | All commands follow unified schema; every player action has explicit feedback | ✅ PASS | command-schema.md defines exact output format for every command |
| IV. Performance | No per-frame Update() for idle income; Profiler data required for hot paths | ✅ PASS | Timer-based TickManager chosen; offline calc is one multiplication on load |

**Post-design re-check**: All gates still pass after Phase 1 design. No violations.

## Project Structure

### Documentation (this feature)

```text
specs/001-hacking-idle-core/
├── plan.md              # This file
├── research.md          # Phase 0 — architecture decisions
├── data-model.md        # Phase 1 — entity definitions
├── quickstart.md        # Phase 1 — end-to-end validation guide
├── contracts/
│   └── command-schema.md  # Phase 1 — terminal command interface contract
├── checklists/
│   └── requirements.md    # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created here)
```

### Source Code (repository root)

```text
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          # Bootstrap, session lifecycle
│   │   ├── TickManager.cs          # Timer-based income tick (not Update)
│   │   ├── SaveSystem.cs           # JSON serialize/deserialize to persistentDataPath
│   │   └── OfflineIncomeCalculator.cs  # Elapsed-time × rate on load
│   ├── Models/
│   │   ├── Location.cs
│   │   ├── Network.cs
│   │   ├── Device.cs
│   │   ├── Malware.cs
│   │   ├── Player.cs
│   │   ├── StoreItem.cs
│   │   ├── Contract.cs
│   │   └── SaveData.cs             # Aggregate save structure
│   ├── Services/
│   │   ├── CommandParser.cs        # Tokenizer + ICommand registry
│   │   ├── Commands/               # One class per command verb
│   │   │   ├── ScanCommand.cs
│   │   │   ├── CrackCommand.cs
│   │   │   ├── InjectCommand.cs
│   │   │   ├── FirewallCommand.cs
│   │   │   ├── ShowCommand.cs
│   │   │   ├── LsCommand.cs
│   │   │   └── CopyCommand.cs
│   │   ├── IncomeService.cs        # Calculates and applies malware income per tick
│   │   ├── StoreService.cs         # Purchase validation and effect application
│   │   ├── ContractService.cs      # Contract availability, progress, completion
│   │   └── LocationService.cs      # Procedural network/device generation
│   ├── Data/
│   │   ├── CurrencyDefinitionSO.cs # ScriptableObject: currency display name, unlock condition
│   │   ├── StoreItemSO.cs          # ScriptableObject: item config
│   │   └── LocationConfigSO.cs     # ScriptableObject: network distribution config
│   ├── UI/
│   │   ├── TerminalController.cs   # Input field → CommandParser → output display
│   │   ├── TerminalOutputView.cs   # Scrollable text output panel
│   │   ├── HUDController.cs        # Balance display, infected device count
│   │   └── StoreController.cs      # Store panel, purchase confirmation
│   └── Interfaces/
│       ├── ICommand.cs             # Execute(string[] args) → CommandResult
│       └── ITickable.cs            # OnTick(double deltaSeconds)
├── Tests/
│   ├── EditMode/
│   │   ├── CommandParserTests.cs
│   │   ├── ScanCommandTests.cs
│   │   ├── CrackCommandTests.cs
│   │   ├── InjectCommandTests.cs
│   │   ├── FirewallCommandTests.cs
│   │   ├── IncomeServiceTests.cs
│   │   ├── OfflineIncomeCalculatorTests.cs
│   │   ├── StoreServiceTests.cs
│   │   ├── ContractServiceTests.cs
│   │   └── SaveSystemTests.cs
│   └── PlayMode/
│       ├── CoreLoopIntegrationTests.cs  # scan → crack → inject → income
│       └── StoreContractIntegrationTests.cs
├── Prefabs/
│   ├── Terminal.prefab
│   └── HUD.prefab
├── Scenes/
│   └── GameScene.unity
├── Resources/
│   └── (LocationConfig SOs, StoreItem SOs, CurrencyDefinition SOs)
└── Settings/                       # URP, Renderer2D (existing)
```

**Structure Decision**: Single Unity project. All game logic under `Assets/Scripts/`
organized by layer (Core/Models/Services/Data/UI). Tests live under `Assets/Tests/`
with separate edit-mode (pure C#, no runtime) and play-mode (full Unity runtime)
assemblies. ScriptableObjects in `Assets/Resources/` for designer-editable config.

## Complexity Tracking

> No constitution violations — section not required.
