# Tasks: Unity → Godot 4.6 Migration

**Branch**: `feature/moving-to-godot` | **Date**: 2026-05-11
**Scope**: All programmatic changes. Manual editor work is tracked in `RecreateUI.md`.

---

## Phase 1: Godot Project Files

**Purpose**: Establish the Godot build and project identity.

- [x] T001 Create `project.godot` (engine config, Autoload for GameManager, input map stubs)
- [x] T002 Create `HackYourWay.csproj` (Godot.NET.Sdk 4.6, .NET 8, C# 12)
- [x] T003 Update `.gitignore` (add `.godot/` cache directory)

---

## Phase 2: Data Resources (ScriptableObject → Resource)

**Purpose**: Convert designer-editable data containers to Godot `Resource` subclasses.

- [x] T004 [P] Convert `Assets/Scripts/Data/StoreItemSO.cs` — `ScriptableObject` → `Resource`, Unity attrs → Godot `[Export]`
- [x] T005 [P] Convert `Assets/Scripts/Data/ContractSO.cs` — same
- [x] T006 [P] Convert `Assets/Scripts/Data/CurrencyDefinitionSO.cs` — same
- [x] T007 [P] Convert `Assets/Scripts/Data/LocationConfigSO.cs` — same

---

## Phase 3: Core Systems

**Purpose**: Replace Unity lifecycle, singleton, serialization, and timer APIs.

- [x] T008 [P] Convert `Assets/Scripts/Core/SaveSystem.cs` — `JsonUtility` → `System.Text.Json`, `Application.persistentDataPath` → `OS.GetUserDataDir()`
- [x] T009 [P] Convert `Assets/Scripts/Core/SlotSaveSystem.cs` — same JSON + path changes
- [x] T010 Convert `Assets/Scripts/Core/TickManager.cs` — `MonoBehaviour` → `Node`, `InvokeRepeating` → `Timer` child node
- [x] T011 Convert `Assets/Scripts/Core/GameManager.cs` — `MonoBehaviour` → `Node` Autoload; lifecycle hooks; `Resources.Load` → `GD.Load`; `Debug.LogWarning` → `GD.PushWarning`

---

## Phase 4: UI Layer

**Purpose**: Replace Unity UI (TMP, InputSystem, UGUI) with Godot Control nodes.

- [x] T012 Convert `Assets/Scripts/UI/TerminalOutputView.cs` — `MonoBehaviour` → `Control`; `TextMeshProUGUI` → `RichTextLabel`; TMP colour tags → BBCode; coroutine scroll → `RichTextLabel.ScrollFollowing`
- [x] T013 Convert `Assets/Scripts/UI/TerminalController.cs` — `MonoBehaviour` → `Control`; `TMP_InputField` → `LineEdit`; `Keyboard.current` polling → `_Input(InputEvent)`; coroutine delay → `_Process` state machine
- [x] T014 Convert `Assets/Scripts/UI/ContractController.cs` — `MonoBehaviour` → `Control`; `Resources.LoadAll` → `DirAccess`; `Instantiate+Transform` → `PackedScene.Instantiate+AddChild`; `TextMeshProUGUI` → `Label`; `Button.onClick` → `Button.Pressed`
- [x] T015 Convert `Assets/Scripts/UI/StoreController.cs` — same pattern as T014
- [x] T016 Convert `Assets/Scripts/UI/HUDController.cs` — `MonoBehaviour` → `Control`; `TextMeshProUGUI` → `Label`; `text` → `Text`

---

## Phase 5: Tests

**Purpose**: Remove Unity Test Framework dependencies; keep NUnit attributes and business logic intact.

- [x] T017 [P] Update 11 EditMode test files — drop `using UnityEngine`; replace `ScriptableObject.CreateInstance<T>()` with `new T()`
- [x] T018 Update PlayMode tests:
  - `CoreLoopIntegrationTests`: `[UnityTest]/IEnumerator` → `[Test]/void`; remove `yield return null`; `ScriptableObject.CreateInstance` → `new`
  - `StoreContractIntegrationTests`: same
  - `TerminalUxIntegrationTests`: pure-C# `[Test]` methods unchanged; `[UnityTest]` scroll tests marked as GDUnit4 integration tests (superseded by RichTextLabel.ScrollFollowing)
  - `PerformanceTests`: `ProfilerRecorder` → `System.Diagnostics.Stopwatch`; `[UnityTest]/IEnumerator` → `[Test]/void`

---

## Phase 6: Speckit Artifacts & Docs

- [x] T019 Update `.specify/memory/constitution.md` — replace Unity-specific engine, testing, and profiling references (version bump MAJOR → 2.0.0)
- [x] T020 Update `CLAUDE.md` — Active Technologies, IDE Setup, Commands section
- [x] T021 Create `RecreateUI.md` — step-by-step Godot editor instructions + manual test guide

---

## Dependencies

- Phase 1 has no dependencies — start immediately
- Phases 2, 3 are independent of each other — can run in parallel
- Phase 4 depends on Phase 3 (GameManager/TickManager APIs referenced by UI)
- Phase 5 depends on Phases 2–4 (tests import game types)
- Phase 6 depends on all above being complete

## Notes

- `[P]` = tasks operate on different files, safe to run in parallel
- Unity files in `Assets/Prefabs/`, `Assets/Scenes/`, `Assets/Settings/`, `Packages/`, `ProjectSettings/` are **left in place** — delete them manually after `RecreateUI.md` steps are complete and the Godot scene builds
- `.meta` files are ignored by Godot and can be deleted at the same time
