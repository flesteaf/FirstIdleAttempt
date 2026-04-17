# Quickstart: Terminal UX Enhancements

Integration and manual-test guide for the three terminal UX features.

## Prerequisites

- Unity 6 (6000.4.2f1) project open
- Play Mode entered via the Unity Editor
- Terminal panel visible on screen with `_scrollRect` and `_inputField` wired up in the Inspector

---

## Scenario 1: Auto-Scroll Smoke Test

**Goal**: Verify the terminal scrolls to the latest line automatically.

1. Enter Play Mode.
2. Click the terminal input field to focus it.
3. Run `ls` on a targeted device that has many files (or trigger any command with long output).
4. **Expected**: The last line of output is visible at the bottom of the scroll area without touching the scrollbar.
5. Run several commands in quick succession (e.g. `scan`, `show networks`, `ls`).
6. **Expected**: After each command the view snaps to the bottom; no mid-frame flicker.

---

## Scenario 2: Autocomplete — Single Match

1. Focus the terminal input field.
2. Type `sc` (do not press Enter).
3. Press **Tab**.
4. **Expected**: Input field now contains `scan`; cursor is at end; no output line appears.

---

## Scenario 3: Autocomplete — Multiple Matches

1. Clear the input field.
2. Type `s`.
3. Press **Tab**.
4. **Expected**: Input field still contains `s`; a suggestion line appears in the output area listing all commands that start with "s" (e.g. `scan  show`).

---

## Scenario 4: Autocomplete — All Commands (Empty Input)

1. Ensure the input field is empty and focused.
2. Press **Tab**.
3. **Expected**: A suggestion line in the output area lists every registered command in alphabetical order.

---

## Scenario 5: Command History — Navigate Back

1. Submit three commands in order: `scan`, `ls`, `inject miner`.
2. Press **Up arrow**.
3. **Expected**: Input field shows `inject miner`.
4. Press **Up arrow** again.
5. **Expected**: Input field shows `ls`.
6. Press **Up arrow** again.
7. **Expected**: Input field shows `scan`.
8. Press **Up arrow** one more time.
9. **Expected**: Input field still shows `scan` (no wrap-around).

---

## Scenario 6: Command History — Navigate Forward to Clear

1. From the state at the end of Scenario 5 (input shows `scan`, `_navIndex == 0`).
2. Press **Down arrow** twice.
3. **Expected after first press**: Input field shows `ls`.
4. **Expected after second press**: Input field shows `inject miner`.
5. Press **Down arrow** once more.
6. **Expected**: Input field is cleared (empty). `_navIndex` reset to -1.

---

## Scenario 7: History Cap (50 entries)

1. Submit 51 unique commands (can use a script or repeat loop in the editor).
2. Navigate all the way back with Up arrow.
3. **Expected**: Only 50 entries are navigable; the first submitted command is gone.

---

## Scenario 8: No Regressions

1. Run all existing edit-mode and play-mode tests in the Unity Test Runner.
2. **Expected**: All tests pass (0 failures).
3. Manually confirm that `ContractController`, `StoreController`, `HUDController` UI panels still function normally after the terminal changes.
