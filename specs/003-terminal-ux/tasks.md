# Tasks: Terminal UX Enhancements

**Input**: Design documents from `specs/003-terminal-ux/`
**Prerequisites**: plan.md ✓ spec.md ✓ research.md ✓ data-model.md ✓ contracts/ ✓ quickstart.md ✓

**Tests**: Included per Constitution Principle II (TDD — tests must be written and confirmed failing before implementation).

**Organization**: Tasks are grouped by user story. US1 (auto-scroll) is fully independent of US2 and US3. US2 and US3 share the `TerminalController` file but touch different methods, so their implementation tasks are sequential.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to
- Exact file paths are included in every task description

---

## Phase 1: Setup (Existing Unity Project)

**Purpose**: Confirm test infrastructure is in place before writing new test files.

- [X] T001 Verify `Assets/Tests/EditMode/` and `Assets/Tests/PlayMode/` directories exist and their `.asmdef` files reference `UnityEngine.TestRunner` and `UnityEditor.TestRunner`; no new packages are required (Unity Input System already present)

---

## Phase 2: Foundational — CommandParser Verb Accessor

**Purpose**: Expose registered command names from `CommandParser` so the autocomplete logic (US2) can query them. This is a service-layer change that must be stable before any `TerminalController` work begins.

**⚠️ CRITICAL**: TDD order — write the failing test first, then implement.

- [X] T002 Write failing edit-mode test class `CommandParserVerbsTests` covering Contract 6 (sorted verb list, read-only result) in `Assets/Tests/EditMode/CommandParserVerbsTests.cs`
- [X] T003 Implement `public IReadOnlyList<string> GetRegisteredVerbs()` on `CommandParser` in `Assets/Scripts/Services/CommandParser.cs` — sorts keys of `_registry` alphabetically and returns them; confirm T002 turns green

**Checkpoint**: `CommandParserVerbsTests` passes. `CommandParser` is ready for autocomplete.

---

## Phase 3: User Story 1 — Auto-Scroll to Latest Output (Priority: P1) 🎯 MVP

**Goal**: Every call to `TerminalOutputView.AppendLine()` scrolls the `ScrollRect` to the bottom at end-of-frame, replacing the existing synchronous `Canvas.ForceUpdateCanvases()` spike.

**Independent Test**: Run Play Mode, submit a command producing more lines than the visible height, confirm the last output line is visible without touching the scrollbar.

- [X] T004 [P] [US1] Write failing play-mode test `TerminalUxIntegrationTests` covering Contract 7 (scroll pending guard: single coroutine scheduled per frame, `_scrollPending` resets after fire) in `Assets/Tests/PlayMode/TerminalUxIntegrationTests.cs`
- [X] T005 [US1] Fix `TerminalOutputView.ScrollToBottom()` in `Assets/Scripts/UI/TerminalOutputView.cs`: add `private bool _scrollPending`, replace `Canvas.ForceUpdateCanvases()` + immediate position set with `if (_scrollPending) return; _scrollPending = true; StartCoroutine(ScrollAtEndOfFrame())`, add private `IEnumerator ScrollAtEndOfFrame()` that yields `WaitForEndOfFrame`, sets `verticalNormalizedPosition = 0f`, sets `_scrollPending = false`; add `using System.Collections;`; confirm T004 turns green

**Checkpoint**: US1 is fully functional. Scroll deferred, no ForceUpdateCanvases, no frame spikes.

---

## Phase 4: User Story 2 — Command Autocomplete (Priority: P2)

**Goal**: Pressing Tab while the input field is focused completes a unique command prefix, lists candidates for ambiguous prefixes, and lists all commands for an empty field.

**Independent Test**: Type `sc` in the terminal input field, press Tab, confirm the field shows `scan` and no suggestion line appears in the output.

- [X] T006 [P] [US2] Extend `TerminalUxIntegrationTests.cs` with play-mode tests for autocomplete covering Contracts 8–11 (single match → complete, multi-match → suggestion line, no match → no-op, empty input → list all) in `Assets/Tests/PlayMode/TerminalUxIntegrationTests.cs`
- [X] T007 [US2] Add Tab autocomplete to `TerminalController` in `Assets/Scripts/UI/TerminalController.cs`: add `private void Update()` checking `_inputField.isFocused` and `Keyboard.current.tabKey.wasPressedThisFrame`; add `private void HandleTab()` that reads `_inputField.text.Trim()`, calls `GameManager.Instance.CommandParser.GetRegisteredVerbs()`, filters by prefix (case-insensitive `StartsWith`), and either completes the field (1 match), appends a suggestion line (2+ matches), or is a no-op (0 matches); after any match, call `EventSystem.current.SetSelectedGameObject(null)` then `_inputField.ActivateInputField()` to retain focus; add `using UnityEngine.InputSystem;` and `using UnityEngine.EventSystems;`; confirm T006 turns green

**Checkpoint**: US2 is fully functional. Tab completes, suggests, or no-ops correctly.

---

## Phase 5: User Story 3 — Command History Navigation (Priority: P3)

**Goal**: Up/Down arrow keys cycle through the last 50 submitted commands. Navigating past the newest entry clears the input field.

**Independent Test**: Submit `scan`, `ls`, `inject miner`; press Up once and confirm `inject miner` appears; press Up twice more and confirm navigation stops at `scan`; press Down three times and confirm the field clears.

- [X] T008 [P] [US3] Write failing edit-mode test class `CommandHistoryTests` covering Contracts 1–5 (add+cap, NavigateBack, NavigateForward, ResetNavigation, Add resets nav) in `Assets/Tests/EditMode/CommandHistoryTests.cs`
- [X] T009 [P] [US3] Extend `TerminalUxIntegrationTests.cs` with play-mode tests for history navigation covering Contracts 12–14 (Up key populates field, Down key clears at end, Submit saves to history) in `Assets/Tests/PlayMode/TerminalUxIntegrationTests.cs`
- [X] T010 [US3] Create `CommandHistory` plain C# class in `Assets/Scripts/Models/CommandHistory.cs`: `private List<string> _entries`, `private int _navIndex = -1`, `public const int MaxEntries = 50`; implement `Add(string)` (append, drop index 0 if over cap, reset `_navIndex`), `NavigateBack()` (decrement `_navIndex` clamped to 0, return entry or null if empty), `NavigateForward()` (increment `_navIndex`; if past end set `_navIndex = -1` return null; else return entry), `ResetNavigation()` (set `_navIndex = -1`); add XML doc on all public members; confirm T008 turns green
- [X] T011 [US3] Wire `CommandHistory` into `TerminalController` in `Assets/Scripts/UI/TerminalController.cs`: add `private readonly CommandHistory _history = new CommandHistory()`; extend `Update()` (already added in T007) with `upArrowKey.wasPressedThisFrame → HandleHistoryUp()` and `downArrowKey.wasPressedThisFrame → HandleHistoryDown()`; implement `HandleHistoryUp()` (call `_history.NavigateBack()`, set `_inputField.text` to result or ignore if null, move caret to end) and `HandleHistoryDown()` (call `_history.NavigateForward()`, set `_inputField.text` to result or `string.Empty` if null, move caret to end); in `OnSubmit()` add `_history.Add(input.Trim())` before clearing the field; confirm T009 turns green

**Checkpoint**: US3 is fully functional. History cap, navigation, and clear on overshoot all work.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Quality gate compliance, profiler evidence, and manual regression validation.

- [ ] T012 Run all Unity edit-mode and play-mode tests in the Unity Test Runner; confirm 0 failures across `CommandParserVerbsTests`, `CommandHistoryTests`, `TerminalUxIntegrationTests`, and all pre-existing test suites
- [ ] T013 [P] Profile `TerminalOutputView.AppendLine()` using the Unity Profiler with a terminal session that produces 60+ output lines; capture a screenshot of the CPU timeline showing no `Canvas.ForceUpdateCanvases` entry and confirm no frame exceeds 22.22 ms (Constitution Principle IV — attach data to PR)
- [ ] T014 [P] Validate all 8 scenarios from `specs/003-terminal-ux/quickstart.md` manually in Play Mode and record pass/fail in the PR description

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1; MUST complete before US2 implementation (T007)
- **US1 (Phase 3)**: Independent of Phase 2 — can start in parallel with Phase 2
- **US2 (Phase 4)**: Depends on Phase 2 (needs `GetRegisteredVerbs()` from T003)
- **US3 (Phase 5)**: Independent of Phase 2 — T008/T010 can start after Phase 1; T011 depends on T010 and can run alongside T007 (different methods in same file — coordinate to avoid merge conflicts)
- **Polish (Phase 6)**: Depends on all story phases complete

### User Story Dependencies

- **US1 (P1)**: No dependency on US2 or US3 — fully independent
- **US2 (P2)**: Depends on Phase 2 (`GetRegisteredVerbs()`); independent of US1 and US3
- **US3 (P3)**: Depends on US2 implementation (T007) being committed first — both T007 and T011 modify `TerminalController.cs`; T011 extends the `Update()` method added by T007

### Within Each User Story

- Tests (T004, T006, T008, T009) MUST be written and confirmed **failing** before their paired implementation task
- Models before controller wiring (T010 before T011)
- `CommandHistory` model (T010) is independently testable via edit-mode before any Play Mode work

### Parallel Opportunities

- T002 and T004 can run in parallel (different files: `CommandParserVerbsTests.cs` vs `TerminalUxIntegrationTests.cs`)
- T008 and T009 can run in parallel (different test approaches: edit-mode vs play-mode)
- T008 and T010 can overlap once T008 tests are written (model implements what tests specify)
- T013 and T014 can run in parallel in Phase 6

---

## Parallel Example: US3

```text
# T008 and T009 can both start once Phase 1 is done:
Task T008: CommandHistoryTests (edit-mode) in Assets/Tests/EditMode/CommandHistoryTests.cs
Task T009: History play-mode tests in Assets/Tests/PlayMode/TerminalUxIntegrationTests.cs

# T010 follows T008 (implement what tests specify):
Task T010: CommandHistory model in Assets/Scripts/Models/CommandHistory.cs

# T011 follows T007 (committed) + T010:
Task T011: Wire history into TerminalController in Assets/Scripts/UI/TerminalController.cs
```

---

## Implementation Strategy

### MVP First (US1 Only — Auto-Scroll)

1. Complete T001 (Phase 1)
2. Complete T004 + T005 (Phase 3 — scroll fix)
3. **STOP and VALIDATE**: Scroll works; no ForceUpdateCanvases; profiler clean
4. Ship or demo auto-scroll independently

### Incremental Delivery

1. Phase 1 → Phase 2 → Phase 3 (US1, MVP) → validate in Play Mode
2. Phase 4 (US2 — autocomplete) → validate Tab in Play Mode
3. Phase 5 (US3 — history) → validate Up/Down in Play Mode
4. Phase 6 (polish + profiler + quickstart)

---

## Notes

- [P] tasks operate on different files — safe to parallelize
- T007 and T011 both modify `TerminalController.cs`; complete T007 first and commit before starting T011
- Constitution Principle II requires each test to be confirmed **failing** before its implementation task; do not skip this step
- Constitution Principle IV requires Profiler evidence (T013) before PR merge
- All three UX features are additive — no existing behaviour is removed; regression risk is low but T012 confirms it
