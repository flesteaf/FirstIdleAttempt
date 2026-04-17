# Research: Terminal UX Enhancements

## Decision 1: Deferred Scroll Strategy

**Decision**: Replace the current synchronous `Canvas.ForceUpdateCanvases()` call in `TerminalOutputView.ScrollToBottom()` with a one-shot end-of-frame coroutine (`yield return new WaitForEndOfFrame()`).

**Rationale**: `Canvas.ForceUpdateCanvases()` forces a full layout rebuild synchronously mid-frame. This is the heaviest Unity UI call — it rebuilds all dirty layout elements across all canvases. On a frame where several `AppendLine` calls arrive (e.g. a tick event + a command response), each call triggers a full rebuild. A coroutine deferral batches all appends into a single layout pass at end-of-frame, matching the comment in the original code ("deferred by one frame") that the implementation did not actually achieve. This eliminates the most common frame-spike source for this feature and satisfies Constitution Principle IV (no hot-path allocations, 22.22 ms budget).

**Implementation note**: `TerminalOutputView` already extends `MonoBehaviour`, so `StartCoroutine` is available. A `bool _scrollPending` guard prevents stacking multiple coroutines when several appends arrive in one frame.

**Alternatives considered**:
- `LateUpdate()` with a dirty flag: Works but adds a permanent `LateUpdate` entry to every frame even when no output is pending. The coroutine approach only runs when triggered.
- Keep `Canvas.ForceUpdateCanvases()`: Violates Principle IV on any frame with multiple appends; scroll is wrong anyway (layout isn't rebuilt for new text height until next frame, so position 0 is calculated against stale dimensions).

---

## Decision 2: Tab Key Interception for Autocomplete

**Decision**: Detect Tab in `TerminalController.Update()` using `UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame`, guarded by a check that the input field is focused (`_inputField.isFocused`). Consume the Tab event (prevent EventSystem from moving focus to the next selectable) by calling `EventSystem.current.SetSelectedGameObject(null)` followed immediately by `_inputField.ActivateInputField()` — effectively re-selecting the same field.

**Rationale**: Unity's EventSystem processes Tab as a "select next" navigation event. By re-selecting the input field in the same frame we intercept Tab, the navigation has no visible effect. This is the lightest approach that requires no subclassing of `TMP_InputField` or custom `InputModule`. The Input System's `wasPressedThisFrame` fires exactly once per press, so there is no repeat-fire issue.

**Alternatives considered**:
- Subclass `TMP_InputField` and override `ProcessEvent`: Works but adds a custom class that must be maintained in sync with TextMeshPro updates.
- Use `StandaloneInputModule` navigation disable: Disabling UI navigation globally breaks other UI elements (store, contract panels).

---

## Decision 3: Up/Down Arrow Key Interception for History Navigation

**Decision**: Detect `Keyboard.current.upArrowKey.wasPressedThisFrame` and `downArrowKey.wasPressedThisFrame` in `TerminalController.Update()`, guarded by `_inputField.isFocused`.

**Rationale**: `TMP_InputField` in single-line mode does not use Up/Down arrow keys for any built-in behaviour (only Left/Right move the caret). Intercepting Up/Down is therefore safe with no conflict. No EventSystem re-selection trick is needed — the input field retains focus naturally.

**Alternatives considered**:
- `onValueChanged` with special sentinel characters: Brittle, not reliable cross-platform.
- Legacy `Input.GetKeyDown`: Works but the project already has Input System installed; mixing both systems requires enabling "Both" mode in Project Settings which adds overhead.

---

## Decision 4: CommandParser Verb Accessor

**Decision**: Add `public IReadOnlyList<string> GetRegisteredVerbs()` to `CommandParser` that returns the sorted list of registered command verb strings. Sorting is done once at call time (not on every `Register` call) — autocomplete is triggered by the player, not by a hot path.

**Rationale**: The smallest surface change that gives `TerminalController` what it needs without exposing the `ICommand` implementations. `IReadOnlyList` prevents callers from modifying the result. Sorting ensures Tab-list output is deterministic (alphabetical) for UX consistency (Constitution Principle III).

**Alternatives considered**:
- Expose the dictionary directly: Breaks encapsulation; callers could clear the registry.
- Build a separate `CommandRegistry` singleton: Over-engineering for a list of ~10 verbs.

---

## Decision 5: CommandHistory Model

**Decision**: Implement `CommandHistory` as a plain C# class (not a MonoBehaviour) in `Assets/Scripts/Models/CommandHistory.cs`. Internal storage is a `List<string>` capped at 50 entries. A separate `int _navIndex` tracks the current Up/Down position. `NavigateBack()` returns the entry at `_navIndex` and decrements; `NavigateForward()` increments and returns `null` when it steps past the newest entry (signals "clear the field").

**Rationale**: A plain C# model is unit-testable in edit-mode without a scene. The cap of 50 is the spec requirement (FR-010); exceeding it drops the oldest entry (`RemoveAt(0)`) and adjusts `_navIndex`. History is session-only (no persistence), so no `ISaveable` interface is needed.

**Alternatives considered**:
- `Queue<string>` or `LinkedList<string>`: Navigation requires random access by index; `List<string>` is the right structure.
- Circular buffer: Unnecessary complexity for a 50-entry cap.
