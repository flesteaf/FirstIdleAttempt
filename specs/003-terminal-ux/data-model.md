# Data Model: Terminal UX Enhancements

## Entities

### CommandHistory

**Location**: `Assets/Scripts/Models/CommandHistory.cs`
**Type**: Plain C# class (no MonoBehaviour)

| Field | Type | Description |
|-------|------|-------------|
| `_entries` | `List<string>` | Ordered list of submitted commands, newest at end. Capped at `MaxEntries`. |
| `_navIndex` | `int` | Current navigation position. `-1` means "not navigating" (fresh state). |
| `MaxEntries` | `const int = 50` | Cap on retained history entries. |

**Behaviour**:
- `void Add(string command)`: Appends command to `_entries`. If count exceeds `MaxEntries`, removes index 0. Resets `_navIndex` to `-1`.
- `string NavigateBack()`: Decrements `_navIndex` (clamped to 0) and returns the entry at that index. Returns `null` if history is empty.
- `string NavigateForward()`: Increments `_navIndex`. If it exceeds `_entries.Count - 1`, resets to `-1` and returns `null` (signals: clear the input field).
- `void ResetNavigation()`: Sets `_navIndex = -1` without clearing history. Called when the player types a new character mid-history-browse.

**State transitions**:
```
Fresh state (_navIndex == -1)
  ↓ NavigateBack()   → _navIndex = _entries.Count - 1  (most recent)
  ↓ NavigateBack()   → _navIndex decrements toward 0 (oldest)
  ↓ NavigateForward() → _navIndex increments back toward Count-1
  ↓ NavigateForward() past newest → _navIndex = -1 (back to fresh, field cleared)
```

---

### TerminalOutputView (extended, not new)

**Location**: `Assets/Scripts/UI/TerminalOutputView.cs`
**Change**: Add `bool _scrollPending` guard field.

| Field | Type | Description |
|-------|------|-------------|
| `_scrollPending` | `bool` | Guards against stacking multiple end-of-frame scroll coroutines. Set to `true` when a coroutine is scheduled; reset to `false` when the coroutine completes. |

**Behaviour change**:
- `ScrollToBottom()` checks `_scrollPending`; if already pending, returns early.
- Otherwise sets `_scrollPending = true` and starts coroutine `ScrollAtEndOfFrame`.
- Coroutine: `yield return new WaitForEndOfFrame()` → set `_scrollRect.verticalNormalizedPosition = 0f` → set `_scrollPending = false`.

---

### CommandParser (extended, not new)

**Location**: `Assets/Scripts/Services/CommandParser.cs`
**Change**: Add `GetRegisteredVerbs()` accessor.

| Method | Signature | Description |
|--------|-----------|-------------|
| `GetRegisteredVerbs` | `IReadOnlyList<string>` | Returns the sorted list of all registered command verb strings. Sort is alphabetical, ascending. |

---

### TerminalController (extended, not new)

**Location**: `Assets/Scripts/UI/TerminalController.cs`
**Change**: Add history and autocomplete wiring.

| Field | Type | Description |
|-------|------|-------------|
| `_history` | `CommandHistory` | Session command history instance. |

**Behaviour additions**:
- `Update()`: Detects Tab, Up, Down key presses when `_inputField.isFocused`.
- `HandleTab()`: Reads current input, resolves prefix against `CommandParser.GetRegisteredVerbs()`, updates input field or appends suggestion line.
- `HandleHistoryUp()`: Calls `_history.NavigateBack()`, sets input field text.
- `HandleHistoryDown()`: Calls `_history.NavigateForward()`, sets input field text or clears it.

## Relationships

```
TerminalController
  ├── owns CommandHistory
  ├── reads CommandParser (via GameManager.Instance.CommandParser)
  └── writes to TerminalOutputView (suggestion lines, command echo)

TerminalOutputView
  └── owns _scrollPending (internal scroll state)

CommandParser
  └── exposes GetRegisteredVerbs() (read-only)
```

## No new persistent data

History is session-only. No changes to `SaveData` or `Player` models are required.
