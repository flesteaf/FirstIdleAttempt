# HackYourWay — Claude Guidance

Auto-generated from all feature plans. Last updated: 2026-05-11

## Active Technologies

- C# 12 (.NET 8, Godot 4.6 runtime) + Godot 4.6, Godot Control nodes (RichTextLabel, LineEdit, Label, Button), GDUnit4 (feature/moving-to-godot)
- JSON flat files via `System.Text.Json` at `OS.GetUserDataDir()/save_{N}.json` (slots 1–7) and `save_index.json` (slot metadata index) (feature/moving-to-godot)

## Project Structure

```text
Assets/Scripts/   ← C# game scripts (migrated to Godot APIs)
Assets/Tests/     ← GDUnit4 + NUnit tests
scenes/           ← Godot .tscn scene files (created in editor)
resources/        ← Godot .tres resource files (created in editor)
  contracts/
  currencies/
  locations/
  store_items/
```

## Commands

```bash
# Open Godot editor (adjust path to your Godot 4.6 installation)
godot --editor

# Run GDUnit4 tests from within Godot editor:
# → Bottom panel → GDUnit4 → Run All Tests

# Build/check C# compilation only:
dotnet build HackYourWay.csproj
```

## Code Style

- All Godot Node/Resource subclasses MUST use the `partial` keyword
- `[Export]` replaces `[SerializeField]` for inspector-exposed fields
- `_Ready()` replaces `Awake()` + `Start()`; `_ExitTree()` replaces `OnDestroy()`
- `_Process(double delta)` replaces `Update()`; use `delta` parameter (not `Time.deltaTime`)
- `_Input(InputEvent @event)` replaces Unity Input System polling
- `OS.GetUserDataDir()` replaces `Application.persistentDataPath`
- `GD.Load<T>("res://path")` replaces `Resources.Load<T>("path")`
- `System.Text.Json.JsonSerializer` with `IncludeFields = true` replaces `JsonUtility`

## Recent Changes

- feature/moving-to-godot: Full engine migration Unity 6 → Godot 4.6; C# 9 → C# 12 / .NET 8; TMP → RichTextLabel/Label; Unity Input System → InputEvent; JsonUtility → System.Text.Json; ScriptableObject → Resource; MonoBehaviour/Control → Node/Control hierarchy; coroutines → _Process state machine; GDUnit4 replaces Unity Test Framework

<!-- MANUAL ADDITIONS START -->
## IDE Setup

- **Rider 2026.1.0.1** works directly with Godot 4.6 C# projects — no Unity-specific package needed.
- Set Rider as the External Editor in Godot: `Editor > Editor Settings > Dotnet > Editor > External Editor`.
- Install the **GDUnit4** plugin via Godot's AssetLib for in-editor test running.
<!-- MANUAL ADDITIONS END -->

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
