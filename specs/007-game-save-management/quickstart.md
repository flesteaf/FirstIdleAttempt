# Quickstart: Game Save Management

**Branch**: `007-game-save-management` | **Date**: 2026-04-28

Developer guide for working with the multi-slot save system.

---

## How to save and load a slot in code

```csharp
// Save current session to slot 3
GameManager.Instance.SlotSaveSystem.SaveSlot(3, GameManager.Instance.SaveData);

// Load slot 2 (full re-bootstrap — replaces all session state)
GameManager.Instance.LoadSlot(2);

// Start a new game (replaces all session state, CurrentSlot = -1)
GameManager.Instance.NewGame();

// Read the slot overview without loading any slot
SaveSlotIndex index = GameManager.Instance.SlotSaveSystem.LoadIndex();
for (int i = 0; i < 7; i++)
{
    SaveSlotInfo info = index.Slots[i];
    string timestamp = info.IsOccupied
        ? new System.DateTime(info.SavedAtUtcTicks, System.DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm")
        : "[empty]";
    Debug.Log($"Slot {info.SlotNumber}: {timestamp}");
}
```

---

## How to add a new command that needs confirmation

1. Inject `ConfirmationService` into the command constructor.
2. In `Execute()`, call `_confirmationService.RequestConfirmation(prompt, onConfirm, onCancel)` and return the prompt as an `Ok` result (the terminal will display it).
3. The terminal routes the next input to the service automatically.

```csharp
public CommandResult Execute(string[] args)
{
    _confirmationService.RequestConfirmation(
        prompt:    "Are you sure? (y/n)",
        onConfirm: () => { /* do destructive action here */ },
        onCancel:  () => { /* nothing to undo */ }
    );
    return CommandResult.Ok("Are you sure? (y/n)");
}
```

> The `Ok` return and the `prompt` string in `RequestConfirmation` must match — the terminal displays the `CommandResult.Message`; the `ConfirmationService` stores the prompt for its `PendingPrompt` property.

---

## How to add a new command that writes to a slot

1. Resolve the slot from `args[0]` using `TryParseSlot()` (see `SaveCommand` for a reference implementation).
2. Call `_slotSaveSystem.SaveSlot(slot, currentSaveData)`.
3. The index is updated automatically inside `SaveSlot`.

```csharp
private bool TryParseSlot(string raw, out int slot)
{
    slot = 0;
    return int.TryParse(raw, out slot) && slot >= 1 && slot <= 7;
}
```

---

## File locations

| File | Purpose |
|------|---------|
| `Application.persistentDataPath/save_index.json` | Slot metadata (occupancy + timestamps) |
| `Application.persistentDataPath/save_N.json` | Full `SaveData` for slot N (absent = empty slot) |
| `Application.persistentDataPath/save.json` | Legacy single-file save (kept after migration) |

---

## Migration from single-file saves

`SlotSaveSystem.MigrateLegacyIfNeeded()` is called automatically from `GameManager.Bootstrap()`. It runs once:

1. Checks if `save.json` exists.
2. Checks if no `save_1.json`–`save_7.json` files exist.
3. If both conditions hold: copies `save.json` → `save_1.json`, writes the index with slot 1 occupied.
4. Sets `GameManager.CurrentSlot = 1`.

The legacy `save.json` is **not deleted** — it remains as a backup.

---

## Writing tests for slot operations

`SlotSaveSystem` accepts a `string basePath` constructor parameter for test isolation. Pass a `Path.GetTempPath()` subdirectory to avoid touching `Application.persistentDataPath`:

```csharp
string testDir = Path.Combine(Path.GetTempPath(), "hyw_test_" + System.Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(testDir);
var sys = new SlotSaveSystem(testDir);

// Test save and load round-trip
var data = new SaveData();
data.Player.AddBalance(CurrencyType.Bitcoin, 42.0);
sys.SaveSlot(1, data);

SaveData loaded = sys.LoadSlot(1);
Assert.AreEqual(42.0, loaded.Player.GetBalance(CurrencyType.Bitcoin));

// Cleanup
Directory.Delete(testDir, recursive: true);
```

---

## ConfirmationService test pattern

```csharp
var svc = new ConfirmationService();
Assert.IsFalse(svc.IsPending);

bool confirmed = false;
svc.RequestConfirmation("Overwrite? (y/n)", () => confirmed = true, () => { });
Assert.IsTrue(svc.IsPending);

svc.Resolve(true);   // simulate "y"
Assert.IsTrue(confirmed);
Assert.IsFalse(svc.IsPending);
```
