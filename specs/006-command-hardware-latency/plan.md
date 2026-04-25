# Implementation Plan: Command Hardware Latency

**Branch**: `feature/006-command-hardware-latency` | **Date**: 2026-04-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/006-command-hardware-latency/spec.md`

## Summary

Replace the flat 1-second universal command delay (`Player.CommandSpeedUpgrade` + `GameManager.GetCommandLatency()`) with per-command timing driven by three player hardware tiers (CPU 1–5, Internet 1–5, GPU 0–3) and per-device hardware properties (CPU 1–5, Bandwidth 1–5). A new `CommandLatencyService` owns all latency math using a **bottleneck model**: network-transfer commands use `min(playerInternetTier, targetBandwidthTier)` as the effective speed, and compute-bound commands (`crack`) use CPU × GPU multipliers. SaveData migrates v2→v3. Eleven new `StoreItemSO` hardware upgrade assets complete the shop tree.

## Technical Context

**Language/Version**: C# 9 (.NET Standard 2.1, Unity 6 scripting runtime)
**Primary Dependencies**: Unity 6 (6000.4.2f1), TextMeshPro, Unity Input System, Unity Test Framework (NUnit)
**Storage**: JSON flat file via `JsonUtility` to `Application.persistentDataPath/save.json`
**Testing**: NUnit edit-mode and play-mode tests via Unity Test Framework
**Target Platform**: PC (Windows, integrated GPU, 4 GB RAM minimum)
**Project Type**: Desktop game (Unity 6, 2D URP)
**Performance Goals**: Stable 45 FPS; no new Update() allocations in hot paths; latency math is pure value-type arithmetic
**Constraints**: `JsonUtility` compatibility (no Dictionary/interfaces as fields), pure C#, no reflection; all public APIs require XML documentation comments; SRP per class; dead code must not be committed

## ⚠️ Spec vs. Research Discrepancy

The spec was revised from a resistance model to a **bottleneck model** (git: "revise spec to bottleneck model"). `research.md` and `data-model.md` (Entity 3 CommandProfile table and Entity 5 `CommandLatencyService`) reflect the **old resistance model** for `inject` and `firewall`. Implementation must follow the current spec:

| Command | Current Spec (bottleneck) | Old Research (resistance) — SUPERSEDED |
|---------|--------------------------|----------------------------------------|
| `inject` | `min(player.BandwidthTier, targetBandwidthTier)` only. Player CPU and target CPU MUST NOT affect inject time (FR-010). | `4.0 × targetCpuResist × targetBwResist / (cpu_speedup × internet_speedup)` |
| `firewall` | `min(player.BandwidthTier, targetBandwidthTier)` only. Target CPU MUST NOT affect firewall time (FR-011). | `2.5 × targetCpuResist / internet_speedup` |
| `ls` | `min(playerInternetTier, targetBandwidthTier)` | Research had `targetBwSpeedup` as multiplier — direction is correct, formula differs |
| `copy` | `min(playerInternetTier, targetBandwidthTier)` × file size | Same issue as ls |

**Action**: `data-model.md` Entity 3 CommandProfile table and Entity 5 `CommandLatencyContext`/`CommandLatencyService` members must be read with this correction. The `TargetCpuResistanceTable` and `TargetBwResistanceTable` entries in the service are **not used**; only `TargetBwSpeedupTable` (for ls/copy) and the bottleneck `min()` calculation (for inject/firewall/scan-ip) are in scope. `CommandLatencyContext.TargetDevice` is still needed (for its `BandwidthTier`); `TargetDevice.CpuTier` is consumed only by `InjectCommand.InjectMiner` to set `Malware.IncomeRate` (FR-022) — not by the latency calculation.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Code Quality | ✅ PASS | `CommandLatencyService`: single responsibility (latency math only). All new public APIs will have XML doc comments. Tier coefficients in named `static readonly` arrays (no magic numbers). `Player.CommandSpeedUpgrade` removed — no dead code. |
| II. Testing Standards | ✅ PASS (with obligation) | New `CommandLatencyServiceTests.cs` required before implementation begins (Red-Green-Refactor). Latency formula for each command category, clamp behaviour, 20× ratio, and bottleneck model must all be tested. |
| III. UX Consistency | ✅ PASS | Progress bar already implemented; per-command durations replace the flat 1 s delay without new UI components. `scan ip` output gains CPU Tier / BW Tier lines, consistent with existing output format. |
| IV. Performance | ✅ PASS | Latency calculations are pure float arithmetic on value types — no heap allocations. No new `Update()` calls. Coroutine already exists in `TerminalController`. |

**Quality Gates pre-merge**:
1. **Build Gate**: Zero compiler errors/warnings in Unity 6 (6000.4.2f1).
2. **Test Gate**: `CommandLatencyServiceTests.cs` (edit-mode, all green). Existing test suite must not regress.
3. **Performance Gate**: No new `Update()` hot paths. Profiler data not required (no new hot paths added).
4. **UX Gate**: `scan ip` output includes CPU/BW tier lines. Progress bar duration matches effective latency.
5. **Constitution Check**: Reviewer confirms all four principles in PR checklist.

## Project Structure

### Documentation (this feature)

```text
specs/006-command-hardware-latency/
├── plan.md              # This file
├── research.md          # Phase 0 — 10 decisions (NOTE: Decisions 2/5 use old resistance model; see § above)
├── data-model.md        # Phase 1 — entity definitions (NOTE: Entity 3/5 use old model; see § above)
├── quickstart.md        # Phase 1 — how to add latency to a new command
├── contracts/
│   └── command-schema.md  # Phase 1 — per-command latency formulas (NOTE: inject/firewall use old model; see §)
└── tasks.md             # Phase 2 (/speckit.tasks output)
```

### Source Code Layout

```text
Assets/Scripts/
├── Core/
│   └── GameManager.cs              # MODIFY: remove GetCommandLatency(); add CommandLatencyService; v2→v3 migration
├── Interfaces/
│   └── ICommand.cs                 # MODIFY: add GetLatency(string[] args) default interface method → 0f
├── Models/
│   ├── Player.cs                   # MODIFY: remove CommandSpeedUpgrade; add CpuTier, InternetTier, GpuTier
│   ├── Device.cs                   # MODIFY: add CpuTier, BandwidthTier
│   ├── SaveData.cs                 # MODIFY: DeviceSaveData += CpuTier, BandwidthTier; SaveData.Version 2→3
│   ├── StoreItem.cs                # MODIFY: add HasHardwareUpgrade, HardwareStatAffected, HardwareTierGranted
│   └── HardwareStat.cs             # NEW: enum { CPU = 0, Internet = 1, GPU = 2 }
├── Data/
│   └── StoreItemSO.cs              # MODIFY: mirror StoreItem hardware upgrade fields + ToModel() mapping
├── Services/
│   ├── CommandLatencyService.cs    # NEW: bottleneck-model latency math with static coefficient tables
│   ├── StoreService.cs             # MODIFY: ApplyEffect() handles hardware tier replacement
│   ├── LocationService.cs          # MODIFY: GenerateLocation() assigns Device.CpuTier, BandwidthTier via PRNG
│   └── Commands/
│       ├── CrackCommand.cs         # MODIFY: override GetLatency (CPU × GPU, by SecurityLevel)
│       ├── ScanCommand.cs          # MODIFY: override GetLatency; show device tiers in scan ip output
│       ├── InjectCommand.cs        # MODIFY: override GetLatency (bottleneck); scale miner rate by device CpuTier
│       ├── FirewallCommand.cs      # MODIFY: override GetLatency (bottleneck)
│       ├── LsCommand.cs            # MODIFY: override GetLatency (bottleneck)
│       └── CopyCommand.cs          # MODIFY: override GetLatency (bottleneck + file size)
├── UI/
│   └── TerminalController.cs       # MODIFY: replace GetCommandLatency() with ICommand.GetLatency(args) lookup

Assets/Tests/EditMode/
└── CommandLatencyServiceTests.cs   # NEW: unit tests for all command categories + 20× ratio + bottleneck model

Assets/Resources/StoreItems/        # 11 NEW StoreItemSO assets (CPU ×4, Bandwidth ×4, GPU ×3)
```

**Structure Decision**: Single-project Unity layout. All new files follow existing namespace/folder conventions. No new assembly definitions needed.

## Complexity Tracking

> No Constitution violations requiring justification.

---

## Appendix: Bottleneck Formula Reference

For implementors — authoritative formulas per current spec (overrides research.md Decision 2 where they conflict):

**Crack** (CPU + GPU only, FR-008):
```
effectiveTime = Clamp(baseTime / (CpuSpeedup[playerCpuTier] × GpuSpeedup[playerGpuTier]), 0.05, maxCap)
```

**All network-transfer commands** (inject, firewall, ls, copy, scan-ip/mac) — **bottleneck model** (FR-009–013, FR-021):
```
effBw         = min(player.BandwidthTier, targetDevice.BandwidthTier)
speedup       = BandwidthSpeedup[effBw]
effectiveTime = Clamp(baseTime / speedup, 0.05, maxCap)          // inject, firewall, ls
effectiveTime = Clamp((baseCopy + fileSizeBytes / RefSize × ScaleFactor) / speedup, 0.05, maxCopy)  // copy
```

**Area scan** (player bandwidth only, no target, FR-009):
```
effectiveTime = Clamp(baseScan / BandwidthSpeedup[player.BandwidthTier], 0.05, maxScan)
```

**Coefficient arrays** (from research.md Decision 2 — valid; resistance arrays discarded):
```
CpuSpeedup[1..5]       = [1.0f, 1.5f, 2.5f, 3.5f, 5.0f]
BandwidthSpeedup[1..5] = [1.0f, 1.5f, 2.3f, 3.2f, 4.0f]   // shared by player and target
GpuSpeedup[0..3]       = [1.0f, 1.6f, 2.8f, 4.0f]
```

**Naming note**: `research.md` and `data-model.md` call the player field `InternetTier`; `tasks.md` corrects this to `Player.BandwidthTier` (matching spec FR-005 wording and `Device.BandwidthTier`). Use `BandwidthTier` everywhere.

**Max ratios** (FR-019, SC-003 verification):
- Crack at CPU5+GPU3 vs CPU1+GPU0: `5.0 × 4.0 = 20×` ✓ (exactly 20×)
- No single axis > 10×: CPU5=5×, GPU3=4×, BW5=4× ✓
- Inject/ls/copy at effBw5 vs effBw1: `4.0 / 1.0 = 4×` (single bandwidth axis max = 4×, well under 10×) ✓
