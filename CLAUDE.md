# HackYourWay — Claude Guidance

Auto-generated from all feature plans. Last updated: 2026-04-29

## Active Technologies
- C# 9 (.NET Standard 2.1, Unity 6 scripting runtime) + Unity 6 (6000.4.2f1), TextMeshPro, Unity Input System, Unity Test Framework (NUnit) (feature/003-terminal-behaviour)
- N/A — history is session-only, no persistence changes (feature/003-terminal-behaviour)
- C# 9 (.NET Standard 2.1, Unity 6 scripting runtime) + Unity 6 (6000.4.2f1), TextMeshPro, Unity Input System, Unity Test Framework (NUnit) (feature/006-command-hardware-latency)
- JSON flat file via `JsonUtility` to `Application.persistentDataPath/save.json` (feature/006-command-hardware-latency)
- C# 9 (.NET Standard 2.1, Unity 6 scripting runtime) + Unity 6 (6000.4.2f1), Unity Test Framework (NUnit) (007-game-save-management)
- JSON flat files via `JsonUtility` at `Application.persistentDataPath/save_{N}.json` (slots 1–7) and `save_index.json` (slot metadata index) (007-game-save-management)

- C# 9 (.NET Standard 2.1, Unity 6 scripting runtime) + Unity 6 (6000.4.2f1), URP 2D, TextMeshPro, Input System, Unity Test Framework (NUnit) (develop)
- JSON flat file via `JsonUtility` to `Application.persistentDataPath/save.json` (develop)

## Project Structure

```text
Assets/Scripts/
Assets/Tests/
```

## Commands

# Add commands for C# 9 (.NET Standard 2.1, Unity 6 scripting runtime)

## Code Style

C# 9 (.NET Standard 2.1, Unity 6 scripting runtime): Follow standard conventions

## Recent Changes
- 007-game-save-management: Added C# 9 (.NET Standard 2.1, Unity 6 scripting runtime) + Unity 6 (6000.4.2f1), Unity Test Framework (NUnit)
- feature/006-command-hardware-latency: Added CommandLatencyService (per-command hardware-tier latency), HardwareStat enum, ICommand.GetLatency default interface method; Device.CpuTier/BandwidthTier; Player.CpuTier/InternetTier/GpuTier; SaveData v2→v3 migration; 11 new StoreItemSO hardware upgrade assets


<!-- MANUAL ADDITIONS START -->
## IDE Setup

- **JetBrains Rider Editor** package MUST be installed via Unity's Package Manager.
- **Rider 2026.1.0.1** MUST be set as the External Script Editor in Unity Preferences
  (`Edit > Preferences > External Tools > External Script Editor`).
<!-- MANUAL ADDITIONS END -->
