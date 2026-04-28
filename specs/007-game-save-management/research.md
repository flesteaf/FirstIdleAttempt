# Research: Game Save Management

**Branch**: `007-game-save-management` | **Date**: 2026-04-28

## Decision 1: Command verb naming

**Decision**: Five top-level commands — `save`, `load`, `delsave`, `newgame`, `saves`

**Rationale**: Short, lowercase, action-verb style matches existing commands (`scan`, `crack`, `copy`, `move`, `forget`). `delsave` avoids future collision with a generic `delete` command. `saves` is a natural noun for "list all slots". `newgame` is unambiguous and self-documenting.

**Alternatives considered**:
- `removesave` / `rm` — too long or too Unix-specific for the game's style
- `loadsave` — redundant ("load" already implies a save file)
- `save list` / `save remove` — sub-command style; the parser uses a flat verb registry and sub-commands would complicate `CommandParser` without benefit

---

## Decision 2: Confirmation prompt mechanism

**Decision**: `ConfirmationService` — a lightweight state machine injected into commands that need multi-turn interaction. Terminal input is intercepted and routed through `ConfirmationService.TryIntercept(input)` before reaching `CommandParser`.

**Rationale**: Keeps `ICommand.Execute()` signature unchanged. Single responsibility: `ConfirmationService` owns pending-confirmation state; the terminal decides routing. Pure C# — testable without a Unity scene.

**Alternatives considered**:
- Two-flag approach (`save 3 -f`) — rejected; spec Q1:B requires an interactive prompt, not a flag
- Async extensions to `ICommand` — over-engineered for a simple yes/no gate
- Returning a special `CommandResult` subtype — would break the `readonly struct` contract and require terminal UI changes to type-check the result

---

## Decision 3: Save slot file layout

**Decision**: One file per slot (`save_1.json` through `save_7.json`) for full save data; a separate `save_index.json` holding a `SaveSlotIndex` wrapper with a `SaveSlotInfo[7]` array for fast metadata reads.

**Rationale**: Deleting slot N is a single file delete (`save_N.json`). Reading the slot overview (`saves` command) only touches the small index file — no need to deserialise 7 full saves. `JsonUtility` cannot serialise a top-level `List<>` or array; wrapping in a class solves this with no extra complexity.

**Alternatives considered**:
- Single `saves.json` with all 7 saves embedded — requires parsing the entire file to display the overview; a single corrupt save blocks all slots
- Directory per slot (`save_3/data.json`) — unnecessary nesting; no benefit over flat files

---

## Decision 4: In-session re-bootstrap on Load

**Decision**: `GameManager.LoadSlot(int slot)` — replaces `SaveData` in place, re-initialises `CommandLatencyService` and `LocationService` from the new data, and calls `RegisterCommands()` again to replace stale command instances.

**Rationale**: No scene reload needed; Unity 6 scene reloads are heavyweight (destroy all MonoBehaviours, re-run `Awake`). Command re-registration is a cheap dictionary replacement. All `ICommand` instances hold a `Player` reference captured at construction time; re-registering creates fresh instances referencing the newly loaded `Player`.

**Alternatives considered**:
- `SceneManager.LoadScene(sceneName)` — clean but slow; breaks terminal output continuity and resets scroll history
- Updating existing command instances via setter injection — requires mutable state on every command class; violates SRP

---

## Decision 5: Active slot tracking and new-game slot state

**Decision**: `GameManager` tracks `CurrentSlot` (int, 1–7, or `-1` for "no active slot"). After `newgame`, `CurrentSlot = -1`. `PersistSession()` is a no-op when `CurrentSlot == -1` — the player must explicitly `save <slot>` to persist. After `save <slot>` or `load <slot>`, `CurrentSlot` is set to that slot number.

**Rationale**: Prevents auto-overwriting an existing slot after `newgame`. Player retains full control over which slot receives their progress. Mirrors the behaviour of most single-player games (new game doesn't commit to a slot until the player saves).

**Alternatives considered**:
- Auto-select slot 1 after `newgame` — could silently overwrite slot 1
- Always prompt for a slot number after `newgame` — adds friction; player can use `save <slot>` at any time anyway

---

## Decision 6: Migration from legacy single-file save

**Decision**: `SlotSaveSystem` checks for a legacy `save.json` at startup. If found and no `save_1.json`–`save_7.json` exist, it copies `save.json` → `save_1.json`, writes the index with slot 1 occupied (timestamp = `SaveData.Player.LastSaveUtcTicks`), and sets `CurrentSlot = 1`.

**Rationale**: Zero data loss for existing players. Migration is transparent and runs once on first boot of the new version. The legacy file is left in place (not deleted) for safety.

**Alternatives considered**:
- Discard legacy save — unacceptable; breaks existing players' progress
- Manual migration prompt — unnecessary friction; migration is unambiguous

---

## Decision 7: SaveData schema version

**Decision**: No schema bump. `SaveData.Version` remains at 3. The per-slot files use the existing `SaveData` schema. All existing v1→v3 migrations apply per-slot file, unchanged.

**Rationale**: The slot system is a file-layout concern, not a data-model change. Schema migration lives in `GameManager.MigrateIfNeeded()` and runs after loading any slot, just as it does today.

**Alternatives considered**:
- Bump to v4 — unnecessary; introduces a migration step that does nothing to the data model itself

---

## Decision 8: `saves` command display format

**Decision**: 7-line plain-text output:
```
Slot 1: 2026-04-28 14:23
Slot 2: [empty]
Slot 3: 2026-04-27 09:05
...
```
Timestamp formatted as `yyyy-MM-dd HH:mm` (UTC) using `new DateTime(ticks, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm")`.

**Rationale**: Matches existing command output style — plain text, no ANSI tables or special characters. 7 lines is compact enough to read at a glance in the terminal window. UTC timestamps are unambiguous and consistent regardless of player time zone.

**Alternatives considered**:
- Local time formatting — differs per machine; unpredictable in multiplayer or cloud contexts
- Compact single-line summary — loses readability when 4+ slots are occupied

---

## Decision 9: `newgame` confirmation always shown

**Decision**: The `newgame` command always shows "Unsaved progress will be lost. Continue? (y/n)" regardless of session state, unless `CurrentSlot == -1` AND no time has elapsed since bootstrapping (practically: always show it).

**Rationale**: Detecting "unsaved progress" precisely requires a dirty-flag system that doesn't yet exist. Always showing the prompt is safe, matches spec Q2:B, and avoids false negatives. The extra keystroke is a minor cost vs. the risk of an accidental reset.

**Alternatives considered**:
- Only show when progress has been made — requires a dirty-flag on `GameManager`; over-engineered for this feature
- Never show confirmation — contradicts spec Q2:B decision

---

## Decision 10: `ConfirmationService` scope and threading

**Decision**: `ConfirmationService` is a plain C# class with no Unity dependencies, holding a single `PendingConfirmation` record (prompt text + `Action onConfirm` + `Action onCancel`). It is constructed in `GameManager.Bootstrap()` and injected into `SaveCommand` and `NewGameCommand`.

**Rationale**: Pure C# means it can be unit-tested in edit mode. A single pending slot is sufficient — only one command can be waiting for confirmation at any time in a single-player terminal. Unity is single-threaded in the main loop, so no locking is needed.

**Alternatives considered**:
- Queue of pending confirmations — unnecessary; two overlapping confirmations cannot occur in this UI model
- MonoBehaviour singleton — couples confirmation logic to Unity lifecycle; untestable in edit mode
