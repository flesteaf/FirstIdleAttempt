# RecreateUI — Godot 4.6 Editor Steps & Manual Test Guide

This document covers everything that **cannot be automated**: scenes, prefabs, resources,
fonts, input map, and renderer settings that must be created or imported through the Godot editor.
Complete these steps **after** the code migration on `feature/moving-to-godot` is merged.

---

## Prerequisites

1. Install Godot 4.6 with .NET support (the "C#" download from godotengine.org).
2. Install **GDUnit4** via Godot → AssetLib → search "GDUnit4" → Install.
3. Open the project: `File > Open Project` → select the repo root (where `project.godot` lives).
4. Godot will import assets and generate `.godot/` on first open — let it finish.

---

## Step 1: Import the Font

1. Copy `Assets/Fonts/ShareTechMono-Regular.ttf` — Godot auto-imports `.ttf` files.
2. In the FileSystem panel, navigate to `Assets/Fonts/`.
3. Click `ShareTechMono-Regular.ttf` → Inspector shows the `FontFile` resource.
4. No extra settings needed. The font is now usable in scenes as a `FontFile`.

---

## Step 2: Create the Directory Structure

In the FileSystem panel, right-click the `res://` root and create these folders:

```
res://
├── scenes/
└── resources/
    ├── contracts/
    ├── currencies/
    ├── locations/
    └── store_items/
```

---

## Step 3: Create Resource Files (.tres)

### 3a. LocationConfig

1. In `res://resources/locations/`, right-click → **New Resource**.
2. Search for `LocationConfigSO` → Create → name it `LocationConfig.tres`.
3. Set values in the Inspector:
   - Min Networks: `2`, Max Networks: `5`
   - Min Devices Per Network: `1`, Max Devices Per Network: `4`
   - Min Files Per Device: `1`, Max Files Per Device: `8`
   - Security Distribution: `[0.15, 0.25, 0.35, 0.25]`
   - Min Ransom Amount: `0.01`, Max Ransom Amount: `0.10`
4. Save (Ctrl+S).

### 3b. Currency Definitions

For each currency, right-click `res://resources/currencies/` → **New Resource** → `CurrencyDefinitionSO`:

| File name | CurrencyType | DisplayName | Symbol |
|-----------|-------------|-------------|--------|
| `Bitcoin.tres` | Bitcoin | Bitcoin | BTC |
| `Monero.tres` | Monero | Monero | XMR |

*(Add additional currencies as defined in `CurrencyType` enum.)*

### 3c. Store Items

For each item, right-click `res://resources/store_items/` → **New Resource** → `StoreItemSO`.
Recreate all 11 hardware upgrade assets plus any software/milestone items from the original
`Assets/Resources/StoreItems/` Unity `.asset` files. Open the old `.asset` files in a text
editor to read their field values (they are plain YAML).

Naming convention: `{category}_{item_name}.tres` (e.g. `hardware_cpu_tier2.tres`).

### 3d. Contracts

For each contract, right-click `res://resources/contracts/` → **New Resource** → `ContractSO`.
Recreate from `Assets/Resources/Contracts/`.

---

## Step 4: Create Row Scenes (Prefab Equivalents)

### 4a. ContractRow.tscn

1. Scene → New Scene.
2. Root node: `HBoxContainer` — rename to `ContractRow`.
3. Add children:
   - `Label` — name it `DescriptionLabel`; set **Horizontal Size Flags** = Expand+Fill.
   - `Label` — name it `RewardLabel`; set min width to ~200 px.
   - `Button` — name it `AcceptButton`; set text to "Accept".
4. Save as `res://scenes/ContractRow.tscn`.

### 4b. StoreItemRow.tscn

1. Scene → New Scene.
2. Root node: `HBoxContainer` — rename to `StoreItemRow`.
3. Add children:
   - `Label` — name it `NameLabel`; Horizontal Size Flags = Expand+Fill.
   - `Label` — name it `PriceLabel`; min width ~200 px.
   - `Button` — name it `BuyButton`; text "Buy".
4. Save as `res://scenes/StoreItemRow.tscn`.

---

## Step 5: Create the Main Scene (terminal_scene.tscn)

### 5a. Scene root

1. Scene → New Scene.
2. Root node: `Control` — rename to `TerminalScene`.
3. Set Anchors Preset to **Full Rect** (so it fills the viewport).

### 5b. Layout structure

```
TerminalScene (Control, Full Rect)
├── VBoxContainer (anchored Full Rect, margin 10px all sides)
│   ├── HeaderBar (HBoxContainer, height 30px)
│   │   ├── HeaderLabel (Label)          ← shows "HACK YOUR WAY · NODE_47"
│   │   ├── BitcoinLabel (Label)         ← BTC balance (HUD)
│   │   └── LocationLabel (Label)        ← current location (HUD)
│   │
│   ├── OutputPanel (RichTextLabel, Vertical Size Flags = Expand+Fill)
│   │   └── (no children)
│   │
│   └── InputBar (HBoxContainer, height 36px)
│       └── InputField (LineEdit, Horizontal Size Flags = Expand+Fill)
│
├── ContractPanel (Panel, docked right or hidden by default)
│   └── VBoxContainer
│       ├── TitleLabel (Label, text "Contracts")
│       └── ContractList (VBoxContainer)  ← _contractListParent
│
└── StorePanel (Panel, docked right or hidden by default)
    └── VBoxContainer
        ├── TitleLabel (Label, text "Store")
        └── ItemList (VBoxContainer)     ← _itemListParent
```

### 5c. Assign the font

For every Label and RichTextLabel:
1. Inspector → **Theme Overrides → Fonts → Font** → drag `ShareTechMono-Regular.ttf`.
2. Set font size to 14 (or match your design).

For the `RichTextLabel` (OutputPanel):
- Enable **BBCode Enabled** = ON.
- Enable **Scroll Following** = ON.
- Set **Clip Contents** = ON so text doesn't overflow.

For the `LineEdit` (InputField):
- Set placeholder text: `"Type a command..."`
- Clear the **Clear Button** option (terminal doesn't need it).

### 5d. Attach scripts

| Node | Script |
|------|--------|
| `TerminalScene` | *(none — or a lightweight scene controller if needed)* |
| `OutputPanel` | `res://Assets/Scripts/UI/TerminalOutputView.cs` |
| `InputField` parent (or TerminalScene) | `res://Assets/Scripts/UI/TerminalController.cs` |
| `TerminalScene` or a HUD child | `res://Assets/Scripts/UI/HUDController.cs` |
| `ContractPanel` | `res://Assets/Scripts/UI/ContractController.cs` |
| `StorePanel` | `res://Assets/Scripts/UI/StoreController.cs` |

### 5e. Wire exported fields

After attaching scripts, select each node and populate the `[Export]` fields in the Inspector:

**TerminalOutputView** (on `OutputPanel`):
- Output Text → drag `OutputPanel` itself (it IS the RichTextLabel)
- Header Label → drag `HeaderLabel`
- Node Id → type `NODE_47` (or your desired identifier)
- System/Command/Output/Error colours → set as desired (defaults are pre-set in script)

**TerminalController** (on its node):
- Input Field → drag `InputField`
- Output View → drag `OutputPanel`

**HUDController** (on its node):
- Bitcoin Label → drag `BitcoinLabel`
- Location Label → drag `LocationLabel`

**ContractController** (on `ContractPanel`):
- Contract List Parent → drag `ContractList` (the VBoxContainer)
- Contract Row Prefab → drag `res://scenes/ContractRow.tscn`
- Terminal Output → drag `OutputPanel`

**StoreController** (on `StorePanel`):
- Item List Parent → drag `ItemList`
- Item Row Prefab → drag `res://scenes/StoreItemRow.tscn`
- Terminal Output → drag `OutputPanel`

### 5f. Save

Save as `res://scenes/terminal_scene.tscn`.

Verify `project.godot` has:
```ini
run/main_scene="res://scenes/terminal_scene.tscn"
```

---

## Step 6: Renderer & Display Settings

1. **Project → Project Settings → Display → Window**:
   - Viewport Width: `1280`, Viewport Height: `720`
   - Stretch Mode: `canvas_items`
   - Stretch Aspect: `expand`

2. **Project → Project Settings → Rendering → Renderer**:
   - Rendering Method: `Forward+` (or `Mobile` for lighter targets)
   - No URP settings needed — Godot manages its own renderer.

3. **Environment**: For a pure UI/terminal game, no 3D environment or sky is needed.
   If Godot adds one by default, remove it via `Scene → World Environment`.

---

## Step 7: Input Map

1. **Project → Project Settings → Input Map** tab.
2. Add the following actions (they are used for future keyboard navigation):

| Action name | Key |
|-------------|-----|
| `ui_up` | Arrow Up |
| `ui_down` | Arrow Down |
| `ui_tab` | Tab |

*(The `TerminalController._Input` implementation handles these keys directly via `Key` enum,
so Input Map entries are optional but useful for rebinding support.)*

---

## Step 8: Autoload Verification

1. **Project → Project Settings → Autoload** tab.
2. Confirm `GameManager` is listed with path `res://Assets/Scripts/Core/GameManager.cs` and **Singleton** = ON.
3. If missing, click `+`, browse to the path, name it `GameManager`, enable Singleton.

---

## Step 9: Clean Up Legacy Unity Files

Once the Godot scene builds and runs:

```bash
# From the repository root — review before deleting
rm -rf Assets/Prefabs/
rm -rf Assets/Scenes/
rm -rf Assets/Settings/
rm -rf Assets/TextMesh\ Pro/
rm -rf Assets/DefaultVolumeProfile.asset*
rm -rf Assets/UniversalRenderPipelineGlobalSettings.asset*
rm -rf Assets/InputSystem_Actions.inputactions*
rm -rf Packages/
rm -rf ProjectSettings/
find . -name "*.meta" -not -path "./.git/*" -delete
```

> **Warning**: Verify that `Assets/Scripts/`, `Assets/Tests/`, and `Assets/Fonts/` are
> NOT deleted — only the Unity-specific directories listed above.

---

## Manual Testing Guide

Run through each scenario after completing the editor steps. Document pass/fail in the PR description.

### Scenario 1 — Boot & Terminal Rendering

| Step | Expected |
|------|----------|
| Launch the game (`F5` in Godot editor) | Scene loads without errors in the Output panel |
| Terminal output area visible | Boot banner appears in green: `╔══...╗ HACK YOUR WAY · NODE_47 ╚══...╝` |
| Type `help` + Enter | All registered commands listed |
| Scroll the output with mouse wheel | Output scrolls; `RichTextLabel.ScrollFollowing` keeps latest line visible |

### Scenario 2 — Command Execution & Latency Bar

| Step | Expected |
|------|----------|
| Type `scan` + Enter | Location scanned; networks listed in output |
| Equip a hardware upgrade (via `StoreController`) and re-run a slow command | Progress bar `[████░░░░░] 60%` animates across frames |
| While bar is animating, type another command | Input dropped silently; bar completes first |

### Scenario 3 — Command History

| Step | Expected |
|------|----------|
| Run three commands in sequence | Commands accepted |
| Press Up arrow | Most-recent command fills input field |
| Press Up again | Previous command |
| Press Down | Moves forward through history |
| Press Down past newest entry | Input field clears |

### Scenario 4 — Tab Autocomplete

| Step | Expected |
|------|----------|
| Type `sc` + Tab | Autocompletes to `scan` |
| Type `s` + Tab | Both `scan` and `show` listed in output |
| Type `xyz` + Tab | Nothing changes (no match) |

### Scenario 5 — Interactive Selection (inject/ls/copy)

| Step | Expected |
|------|----------|
| Type `inject miner` (without IP) | Numbered list of devices appears in output |
| Press Down arrow | Highlight moves down (`>` prefix shifts) |
| Press Enter | Highlighted device selected; miner injected |
| Type `inject miner` again, type `0` or out-of-range text | Selection cancelled gracefully |

### Scenario 6 — Save / Load / Slot Management

| Step | Expected |
|------|----------|
| Type `save 1` + Enter | Save written to `OS.GetUserDataDir()/save_1.json`; confirmation message shown |
| Quit and relaunch | `save_1.json` auto-loaded (slot migration from previous `save.json` if present) |
| Type `saves` | Slot list shows slot 1 as occupied with timestamp |
| Type `load 1` | Save data restored; location and balances match what was saved |
| Type `newgame`, confirm with `y` | Game resets to default state; slot set to -1 |
| Type `delsave 1`, confirm | Slot 1 deleted from index |

### Scenario 7 — Store & Contracts

| Step | Expected |
|------|----------|
| Open the Store panel | Items loaded from `res://resources/store_items/`; name + price visible |
| Click "Buy" on an affordable item | Purchase message in terminal; balance deducted |
| Click "Buy" on an item you can't afford | Error message displayed |
| Open the Contracts panel | Available contracts loaded from `res://resources/contracts/` |
| Click "Accept" on a contract | Contract moves to Active list |

### Scenario 8 — HUD Updates

| Step | Expected |
|------|----------|
| Earn BTC (inject miner, wait a tick) | `BTC: 0.0010` label updates without frame drops |
| Move to a new location (`move`) | `LOC: node_xxx` label updates on next tick |

### Scenario 9 — Performance Baseline

| Step | Expected |
|------|----------|
| Open Godot's built-in Profiler while 10+ miners are active | CPU time per frame stays below 22.22 ms |
| Run `ls` on a device with 100+ files | Output renders instantly; no noticeable stutter |

### Scenario 10 — Offline Income

| Step | Expected |
|------|----------|
| Install a miner, save to slot 1, close game | Save file written |
| Wait 30 seconds, relaunch | On load, offline income for ~30s credited; message shown in terminal |

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---------|-------------|-----|
| `[Export]` fields show `null` in Inspector | Script not attached or not saved | Re-attach script and save the scene |
| `GameManager not initialised` error | Autoload not configured | Verify GameManager in Project → Autoload |
| `LocationConfig.tres not found` warning | Resource not created | Complete Step 3a |
| `ContractRow.tscn` instantiates empty | Node names don't match expected (`DescriptionLabel` etc.) | Verify names exactly match Step 4a |
| BBCode not rendering (raw `[color=…]` visible) | `BbcodeEnabled = false` | Set `BbcodeEnabled = true` on OutputPanel RichTextLabel |
| Arrow keys move cursor instead of history | `_Input` consumed by LineEdit before script | Check `GetViewport().SetInputAsHandled()` is called |
