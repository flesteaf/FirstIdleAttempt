# Data Model: Game Save Management

**Branch**: `007-game-save-management` | **Date**: 2026-04-28

---

## Entity 1: SaveSlotInfo

Lightweight metadata record stored in the slot index. Does **not** contain full game state — only the information needed to display the `saves` overview without deserialising every slot file.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `SlotNumber` | int | 1–7 | Identifies which slot this entry describes |
| `IsOccupied` | bool | — | True if a save file exists for this slot |
| `SavedAtUtcTicks` | long | ≥ 0 | `DateTime.UtcNow.Ticks` at the moment of last save; 0 when empty |

**Serialisation**: `[System.Serializable]` C# class; compatible with `JsonUtility`.

**Display rule**: Format `SavedAtUtcTicks` as `new DateTime(ticks, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm")` in the `saves` command output.

---

## Entity 2: SaveSlotIndex

Top-level wrapper holding all 7 slot metadata records. Stored as `save_index.json`.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Slots` | `SaveSlotInfo[]` | Length = 7, indices 0–6 | `Slots[0]` → slot 1, `Slots[6]` → slot 7 |

**Invariant**: `Slots` is always initialised to length 7. `Slots[i].SlotNumber == i + 1` for all i.

**Serialisation**: `[System.Serializable]` class; `JsonUtility` can serialise fixed-length arrays as fields on a class.

---

## Entity 3: SlotSaveSystem

Service class that owns all multi-slot file operations. Wraps path construction and atomic metadata updates.

| Responsibility | Method | Description |
|----------------|--------|-------------|
| Load a slot | `LoadSlot(int slot) : SaveData` | Reads `save_{slot}.json`; returns `null` if file absent |
| Save to a slot | `SaveSlot(int slot, SaveData data)` | Writes `save_{slot}.json`; updates index entry |
| Delete a slot | `DeleteSlot(int slot)` | Deletes `save_{slot}.json`; marks index entry as empty |
| Read index | `LoadIndex() : SaveSlotIndex` | Reads `save_index.json`; returns default if absent |
| Write index | `SaveIndex(SaveSlotIndex index)` | Writes `save_index.json` |
| Migrate legacy | `MigrateLegacyIfNeeded()` | Copies `save.json` → `save_1.json` on first boot if no slot files exist |

**Slot range**: All public methods validate `slot` is in [1, 7] and throw `ArgumentOutOfRangeException` on violation — callers (commands) are responsible for surfacing this as a user error.

**File layout**:
```
Application.persistentDataPath/
├── save_index.json     # SaveSlotIndex (7 SaveSlotInfo entries)
├── save_1.json         # SaveData for slot 1 (absent if slot is empty)
├── save_2.json
...
└── save_7.json
```

---

## Entity 4: ConfirmationService

Pure C# state machine managing one in-flight yes/no confirmation at a time. No Unity dependencies.

| Field/Method | Type | Description |
|---|---|---|
| `IsPending` | bool (get) | True when a command is waiting for y/n input |
| `PendingPrompt` | string (get) | The prompt text to display (null when not pending) |
| `RequestConfirmation(string prompt, Action onConfirm, Action onCancel)` | void | Registers a pending confirmation; replaces any existing one |
| `Resolve(bool confirmed)` | void | Calls `onConfirm` or `onCancel`; clears pending state |
| `Cancel()` | void | Clears pending state without invoking either callback |

**Invariant**: At most one confirmation is pending at any time. Calling `RequestConfirmation` while `IsPending` is true replaces the previous pending action (effectively cancelling it).

**Thread safety**: Not required — Unity runs on the main thread; all calls occur synchronously within a single frame's input event.

---

## Entity 5: GameManager additions

Changes to the existing `GameManager` class to support slot-aware session management.

| Addition | Type | Description |
|----------|------|-------------|
| `CurrentSlot` | int property | Active slot (1–7) or -1 (no active slot, e.g. after `newgame`) |
| `SlotSaveSystem` | `SlotSaveSystem` field | Replaces direct `SaveSystem` usage for slot-aware operations; `SaveSystem` still used internally by `SlotSaveSystem` |
| `ConfirmationService` | `ConfirmationService` property | Injected into `SaveCommand` and `NewGameCommand`; accessible for terminal routing |
| `LoadSlot(int slot)` | void | Loads slot data, applies migration, re-initialises services, re-registers commands. Sets `CurrentSlot = slot`. |
| `NewGame()` | void | Resets `SaveData` to `CreateDefault()`, re-initialises services, re-registers commands. Sets `CurrentSlot = -1`. |

**`PersistSession()` update**: Guard added — skips save when `CurrentSlot == -1`.

---

## Entity 6: Command classes (new)

Five new `ICommand` implementations. All are pure C# classes testable in edit mode.

| Class | Verb | Constructor dependencies |
|-------|------|--------------------------|
| `SaveCommand` | `save` | `SlotSaveSystem`, `ConfirmationService`, `Func<SaveData>` (current-data provider), `Action<int>` (set-active-slot callback) |
| `LoadCommand` | `load` | `SlotSaveSystem`, `Action<int>` (load-slot callback into GameManager) |
| `DelsaveCommand` | `delsave` | `SlotSaveSystem` |
| `NewGameCommand` | `newgame` | `ConfirmationService`, `Action` (new-game callback into GameManager) |
| `SavesCommand` | `saves` | `SlotSaveSystem` |

**Notes**:
- `SaveCommand` requests confirmation via `ConfirmationService` when the target slot is occupied. The actual save call is deferred to the `onConfirm` callback.
- `LoadCommand` calls the `Action<int>` callback (i.e. `GameManager.LoadSlot`) immediately after validating the slot is occupied — no confirmation needed.
- `DelsaveCommand` validates the slot is occupied and calls `SlotSaveSystem.DeleteSlot` directly.
- `NewGameCommand` always requests confirmation via `ConfirmationService`, then calls the `Action` callback (i.e. `GameManager.NewGame`).
- `SavesCommand` calls `SlotSaveSystem.LoadIndex()` and formats the 7-line display.

---

## State Transitions

```
[No active slot / CurrentSlot = -1]
    ──save <N>──────────────────────► [Active slot = N]
    ──load <N>──────────────────────► [Active slot = N]

[Active slot = N]
    ──save <N> (same slot, confirm)──► [Active slot = N, data updated]
    ──save <M> (different slot)──────► [Active slot = M, N slot still exists]
    ──load <M> (confirmed)───────────► [Active slot = M, full re-bootstrap]
    ──newgame (confirmed)────────────► [No active slot / CurrentSlot = -1]
    ──delsave <N> (current slot)─────► [Slot N deleted, CurrentSlot = -1]
    ──delsave <M> (other slot)───────► [Slot M deleted, CurrentSlot unchanged]
```

**Note**: Loading a slot always triggers a full in-session re-bootstrap (services re-initialised, commands re-registered). The terminal continues displaying — scroll history is preserved.
