# Command Schema: Game Save Management

**Branch**: `007-game-save-management` | **Date**: 2026-04-28

All commands follow the existing terminal conventions: lowercase verb, space-separated args, plain-text output, `Error: <message>` prefix on failures.

---

## `save <slot>`

Saves the current game state to the specified slot.

**Syntax**: `save <slot>` where `<slot>` is an integer 1–7.

| Scenario | Input example | Output |
|----------|---------------|--------|
| Empty slot | `save 3` | `Game saved to slot 3.` |
| Occupied slot (prompt) | `save 3` | `Slot 3 is occupied, overwrite? (y/n)` |
| Occupied slot — confirm | `y` (after prompt) | `Game saved to slot 3.` |
| Occupied slot — decline | `n` (after prompt) | `Save cancelled.` |
| Out-of-range slot | `save 0` or `save 8` | `Error: Slot must be between 1 and 7.` |
| Missing argument | `save` | `Error: Usage: save <slot>` |
| Non-integer argument | `save abc` | `Error: Slot must be between 1 and 7.` |

**Side effects**:
- Writes `save_{slot}.json` (full `SaveData`).
- Updates `save_index.json` entry for the slot (sets `IsOccupied = true`, updates `SavedAtUtcTicks`).
- Sets `GameManager.CurrentSlot = slot`.

**Latency**: 0 (instant command — file I/O is fast enough to be imperceptible; no progress bar).

---

## `load <slot>`

Loads a previously saved game from the specified slot. Replaces all current session state.

**Syntax**: `load <slot>` where `<slot>` is an integer 1–7.

| Scenario | Input example | Output |
|----------|---------------|--------|
| Occupied slot | `load 2` | `Game loaded from slot 2.` |
| Empty slot | `load 6` | `Error: Slot 6 is empty.` |
| Out-of-range slot | `load 9` | `Error: Slot must be between 1 and 7.` |
| Missing argument | `load` | `Error: Usage: load <slot>` |
| Non-integer argument | `load abc` | `Error: Slot must be between 1 and 7.` |

**Side effects**:
- Reads `save_{slot}.json`.
- Applies schema migration (`MigrateIfNeeded`) to the loaded data.
- Re-initialises `LocationService` and `CommandLatencyService`.
- Re-registers all commands in `CommandParser` with fresh instances referencing the loaded `Player`.
- Sets `GameManager.CurrentSlot = slot`.

**Latency**: 0 (instant).

---

## `delsave <slot>`

Removes an existing save from the specified slot. The slot becomes empty.

**Syntax**: `delsave <slot>` where `<slot>` is an integer 1–7.

| Scenario | Input example | Output |
|----------|---------------|--------|
| Occupied slot | `delsave 4` | `Save slot 4 removed.` |
| Empty slot | `delsave 1` | `Error: Slot 1 has no save to remove.` |
| Out-of-range slot | `delsave 0` | `Error: Slot must be between 1 and 7.` |
| Missing argument | `delsave` | `Error: Usage: delsave <slot>` |
| Non-integer argument | `delsave xyz` | `Error: Slot must be between 1 and 7.` |
| Deleting the active slot | `delsave 3` (when `CurrentSlot == 3`) | `Save slot 3 removed.` (then `CurrentSlot = -1`) |

**Side effects**:
- Deletes `save_{slot}.json`.
- Updates `save_index.json` entry (sets `IsOccupied = false`, `SavedAtUtcTicks = 0`).
- If `slot == GameManager.CurrentSlot`, sets `CurrentSlot = -1`.

**Latency**: 0 (instant).

---

## `newgame`

Resets the game to the default initial state. Prompts for confirmation before resetting.

**Syntax**: `newgame` (no arguments).

| Scenario | Input example | Output |
|----------|---------------|--------|
| Confirmation prompt | `newgame` | `Unsaved progress will be lost. Continue? (y/n)` |
| Confirmed reset | `y` (after prompt) | `New game started.` |
| Cancelled reset | `n` (after prompt) | `New game cancelled.` |
| Unexpected arguments (ignored) | `newgame foo` | `Unsaved progress will be lost. Continue? (y/n)` |

**Side effects (on confirm)**:
- Replaces `SaveData` with `CreateDefault()`.
- Re-initialises `LocationService` and `CommandLatencyService`.
- Re-registers all commands in `CommandParser`.
- Sets `GameManager.CurrentSlot = -1`.
- Does **not** modify any slot files or the index — existing saves are preserved.

**Latency**: 0 (instant).

---

## `saves`

Displays the status of all 7 save slots.

**Syntax**: `saves` (no arguments).

**Output format** (always 7 lines):
```
Slot 1: 2026-04-28 14:23
Slot 2: [empty]
Slot 3: 2026-04-27 09:05
Slot 4: [empty]
Slot 5: [empty]
Slot 6: [empty]
Slot 7: 2026-04-25 21:47
```

- Timestamp format: `yyyy-MM-dd HH:mm` (UTC).
- `[empty]` for slots with no save.
- Current active slot is not visually distinguished (no asterisk or marker) — keeping the UI simple.

**Side effects**: None (read-only).

**Latency**: 0 (instant).

---

## Terminal Routing for Confirmations

When `ConfirmationService.IsPending == true`, the terminal's input handler must route the next input to `ConfirmationService.Resolve()` instead of `CommandParser.Parse()`.

**Routing logic** (pseudo-code):
```
on player input:
  if ConfirmationService.IsPending:
    ConfirmationService.Resolve(input.Trim().ToLower() == "y")
  else:
    result = CommandParser.Parse(input)
    Display(result)
```

**Confirmation display**: The terminal displays `ConfirmationService.PendingPrompt` as a `CommandResult.Ok` output when the prompt is first issued. The `(y/n)` suffix is part of the prompt string.

**Cancel by re-command**: If the player types any full command while a confirmation is pending, that input is treated as "n" (cancel) and the command is not executed. (Implementation: `ConfirmationService.Resolve(false)` is called first; the input is not re-routed to the parser.)

> **Rationale**: Prevents a player from accidentally running a different command when they meant to type "n". The prompt remains active until explicitly resolved.
