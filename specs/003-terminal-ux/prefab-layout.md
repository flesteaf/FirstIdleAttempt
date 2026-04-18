# Terminal.prefab Layout Reference

Last updated: 2026-04-18 (after HUD styling fix)

## Root: Terminal (Canvas)

**Canvas** — Screen Space Overlay  
**CanvasScaler** — Constant Pixel Size, ScaleFactor 1, ReferenceResolution 800×600  
**TerminalController** — `_inputField` → CommandInput TMP_InputField, `_outputView` → OutputPanel TerminalOutputView

### Child order

| # | Name         | Active | Purpose                          |
|---|--------------|--------|----------------------------------|
| 1 | OutputPanel  | yes    | Scrollable terminal output area  |
| 2 | CommandInput | yes    | Text input field at bottom       |
| 3 | HUD          | yes    | BTC balance strip (top-right)    |
| 4 | Separator    | yes    | 2px horizontal divider line      |
| 5 | Header       | **no** | "HACK YOUR WAY · NODE_XX" bar    |

---

## OutputPanel

| Property | Value |
|---|---|
| Anchor | (0,1) → (1,1) — stretch-X, top |
| AnchoredPosition | (0, -510) |
| SizeDelta | (0, 1020) |
| Image color | `#0D1A0D` — `(0.051, 0.102, 0.051, 1)` |
| Image sprite | Unity UISprite (built-in), Type: Sliced |

**TerminalOutputView** fields:
- `_nodeId` = `"NODE_47"`
- `_headerLabel` → Header's TMP child label
- `_systemColor`  = `(0.4, 0.85, 0.4, 1)`
- `_commandColor` = `(0.0, 1.0, 0.25, 1)`
- `_outputColor`  = `(0.85, 0.95, 0.85, 1)`
- `_errorColor`   = `(1.0, 0.3, 0.3, 1)`

**ScrollRect** — vertical only; horizontal and vertical scrollbars present but auto-hidden.  
Scrollbar colors are still default white — styling TODO.

---

## CommandInput

| Property | Value |
|---|---|
| Anchor | (0,0) → (1,0) — stretch-X, bottom |
| AnchoredPosition | (0, 29) |
| SizeDelta | (0, 58) |
| Placeholder text | "Enter text..." |

---

## HUD

| Property | Value |
|---|---|
| Anchor | (1,1) → (1,1) — top-right corner |
| AnchoredPosition | (-85, -14) |
| SizeDelta | (160, 28) |
| Pivot | (0.5, 0.5) |
| Image color | `#0A2A0A` — `(0.039, 0.165, 0.039, 1)` — matches Header |
| Image sprite | Unity UISprite, Type: Sliced |

**HUDController** — `_bitcoinLabel` → BTCLabel TMP child  
**HUDController.cs** formats balance as `BTC: {value:F4}` and caches last value to skip redundant refreshes.

### BTCLabel (child of HUD)

| Property | Value |
|---|---|
| Anchor | (0,0) → (1,1) — stretch-fill parent |
| AnchoredPosition | (0, 0) |
| SizeDelta | (-8, -4) — 4px h-padding, 2px v-padding |
| Font | Terminal monospace (guid `8f586378b4e144a9851e7b34d9b748ee`) |
| Font size | 14 |
| Color | `(0, 1, 0.25, 1)` — matches `_commandColor` |
| Alignment | Center horizontal, Middle vertical |
| Word wrap | Disabled |

---

## Separator

| Property | Value |
|---|---|
| Anchor | (0,0) → (1,0) — stretch-X |
| AnchoredPosition | (0, 58) |
| SizeDelta | (0, 2) |
| Image color | `(0.118, 0.227, 0.118, 1)` — medium green |

---

## Header *(currently inactive)*

Re-enable in Inspector to restore the title bar. Sits at the same Y (-14) and height (28) as the HUD, so both form a visual top bar when Header is active.

| Property | Value |
|---|---|
| Anchor | (0.5,1) → (0.5,1) — center-top |
| AnchoredPosition | (400, -14) |
| SizeDelta | (800, 28) |
| Image color | `(0.039, 0.165, 0.039, 1)` — same as HUD |
| Image sprite | none — plain solid fill |

Child TMP label text is set at runtime by `TerminalOutputView.Start()`:  
`"HACK YOUR WAY  ·  {_nodeId}"`

---

## Known font limitations

The terminal font (`8f586378b4e144a9851e7b34d9b748ee`) does **not** support:
- Unicode box-drawing characters (`╔ ═ ╗ ║ ╚ ╝`) — `PrintBoot()` box art does not render
- `›` (U+203A) — command echo prefix falls back to `>`

Either switch to a font with full Unicode coverage or replace both with ASCII equivalents.
