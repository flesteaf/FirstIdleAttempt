# Research: Command Hardware Latency

**Feature**: `specs/006-command-hardware-latency` | **Date**: 2026-04-25

---

## Decision 1 — Latency formula shape

**Decision**: Multiplicative. `effectiveTime = Clamp(baseTime × targetResistance / playerSpeedup, minFloor, maxCap)`.

- `playerSpeedup` is the product of all applicable player-hardware speedup factors.
- `targetResistance` is the product of all applicable target-device resistance factors.
- `minFloor = 0.05f` (FR-003).
- `maxCap` is defined per command (FR-003).

**Rationale**: Multiplicative models produce smooth, predictable progression curves. Dividing by speedup (>1) naturally asymptotes toward the floor; multiplying by resistance (≥1) naturally asymptotes toward the cap. Additive models create discontinuities at tier boundaries and require bespoke capping logic.

**Alternatives considered**: Additive (each tier subtracts fixed seconds) — rejected because subtracting near-constant amounts makes high tiers feel identical; percentage-based linear (each tier reduces by X%) — equivalent to multiplicative but requires extra arithmetic.

---

## Decision 2 — Coefficient values and 20× target

**Decision**: Three coefficient arrays satisfy `max_crack = CPU5 × GPU3 = 5.0 × 4.0 = 20×` and `max_inject = CPU5 × Internet5 = 5.0 × 4.0 = 20×`.

```
cpu_speedup[1..5]      = [1.0, 1.5, 2.5, 3.5, 5.0]   // crack + inject (player side)
internet_speedup[1..5] = [1.0, 1.5, 2.3, 3.2, 4.0]   // scan / inject / firewall / ls / copy
gpu_speedup[0..3]      = [1.0, 1.6, 2.8, 4.0]         // crack only; 0 = no GPU

target_cpu_resistance[1..5]  = [1.0, 1.1, 1.3, 1.6, 2.0]  // inject + firewall resistance
target_bw_speedup[1..5]      = [1.0, 1.2, 1.5, 1.8, 2.2]  // ls + copy benefit
target_bw_resistance[1..5]   = [1.0, 1.05, 1.12, 1.20, 1.30]  // inject added resistance
target_bw_scan_overhead[1..5]= [1.0, 1.05, 1.10, 1.15, 1.20]  // scan ip/mac overhead
```

**Verification**:

| Axis | Max factor | Passes ≤10× rule |
|------|-----------|-----------------|
| CPU5 (crack/inject) | 5.0 | ✓ |
| GPU3 (crack) | 4.0 | ✓ |
| Internet5 (all network cmds) | 4.0 | ✓ |
| Combined crack (CPU5 × GPU3) | 20.0 | ✓ |
| Combined inject (CPU5 × Internet5) | 20.0 | ✓ |

Target-device resistance cancels in the ratio comparison (same target for both base and max player), so the 20× ratio holds regardless of target tier.

**SC-005 check**: `target_cpu_resistance[5] = 2.0` → 100% overhead at tier 5. ✓ (SC-005 cap = 100%).

**FR-021 check**: All `*[1] = 1.0` → tier-1 devices contribute zero overhead. ✓

**Rationale**: Geometric spacing between tiers (roughly ×1.4 per step for Internet) means each upgrade feels roughly equally impactful. GPU tiers are steeper because the GPU is a specialised, high-cost purchase (User Story 4).

**Alternatives considered**: Equal-step linear (each tier adds fixed multiplier) — rejected because early tiers feel large and late tiers feel small; exponential with equal base — produces too steep a curve at top tiers.

---

## Decision 3 — SC-001 single-axis resolution

**Decision**: SC-001 ("every command at least 20×") is satisfied for multi-axis commands only. `scan area` (internet-only) achieves a maximum of 4× player speedup; `firewall` on any target similarly achieves at most 4×. This is a known spec tension between FR-019/SC-001 and the no-single-axis->10× rule.

**Resolution**: SC-001 is interpreted as the design ceiling for the combined hardware system, not a per-command minimum guarantee. The 20× is visible on `crack` and `inject`, which reward investment in multiple upgrade types. Single-axis commands scale proportionally to the relevant axis (up to 4× at max internet, up to 5× at max CPU for commands that are CPU-only — none exist in this feature). 

The spec motivation (SC-003) is preserved: no single axis delivers 10× alone, so players who buy only CPU or only internet receive a partial benefit, incentivising full investment.

---

## Decision 4 — ICommand.GetLatency

**Decision**: Add a default-implementation interface method `float GetLatency(string[] args) => 0f` to `ICommand`. Instant commands (show, help, forget, move) inherit the default. Latency commands override.

**Rationale**: C# 8 default interface methods are supported in .NET Standard 2.1 and Unity 6 (IL2CPP target uses Roslyn C# 9). No abstract base class is needed. All existing commands stay compilable with no change.

**Alternatives considered**: Abstract base class `CommandBase : ICommand` — adds inheritance coupling for all 10+ existing commands with no other benefit; separate `ILatencyCommand` interface — requires `is` casting in TerminalController; `CommandResult` carrying the latency — delays availability to after execution (TerminalController needs it before executing).

---

## Decision 5 — CommandLatencyService: static vs injectable

**Decision**: `CommandLatencyService` is a **non-static class** instantiated once in `GameManager.Bootstrap()`. It holds a reference to `Player` and exposes `CalculateLatency(CommandLatencyContext context)`. Commands receive it via constructor injection alongside their existing dependencies.

Coefficient arrays and base-time constants live as `private static readonly float[]` inside the service (no ScriptableObject needed for coefficients — they are balance-sensitive but do not need runtime designer editing without a full rebuild).

**Rationale**: Static class can't hold `Player` reference (Player is loaded at runtime). Non-static makes unit testing possible (instantiate with a test Player). No ScriptableObject for coefficients keeps the implementation purely code-side, avoiding asset-creation overhead and `Resources.Load` failure modes.

**Alternatives considered**: Separate `CommandLatencyConfigSO` per command verb — excessive number of assets (9 verbs × SO creation); embed latency in each command with no shared service — duplicates coefficient tables; static helper with no Player — requires passing Player everywhere.

---

## Decision 6 — Device tier PRNG placement

**Decision**: In `LocationService.GenerateLocation()`, `CpuTier` and `BandwidthTier` are assigned **after** all existing `rng.Next()` calls for a given device (i.e., after file generation). Two `rng.Next(1, 6)` calls per device are appended at the end of the inner device loop.

**Rationale**: Placing new RNG calls AFTER existing calls preserves all existing device properties (Ip, Mac, FirewallStatus, open ports, files) for all existing seeds, making the change backward-compatible. New calls at the end only add new state; they don't shift the existing sequence.

**Alternatives considered**: Placing tiers BEFORE existing calls — shifts the entire RNG sequence, changing Ip/Mac/files for all existing seeds; generating tiers as a function of the seed directly (e.g., `seed % 5 + 1`) — not random enough, biased pattern.

---

## Decision 7 — Save schema v2 → v3

**Decision**: Bump `SaveData.Version` to 3. Add:
- `Player.CpuTier` (int, persists in existing `Player` serializable class)
- `Player.InternetTier` (int)
- `Player.GpuTier` (int)
- `DeviceSaveData.CpuTier` (int)
- `DeviceSaveData.BandwidthTier` (int)

JsonUtility defaults missing int fields to `0`. The v2→v3 migration in `GameManager.MigrateIfNeeded()` sets:
- `Player.CpuTier == 0` → 1 (base tier, not zero)
- `Player.InternetTier == 0` → 1 (base tier)
- `Player.GpuTier == 0` → 0 (no GPU is correct default; no migration needed)
- Per-device: `CpuTier == 0` → 1, `BandwidthTier == 0` → 1 (old saves get neutral tier-1 devices)

**Rationale**: GpuTier 0 is the correct baseline (no GPU) so JsonUtility default is already correct. Old saves are given the neutral device baseline (tier 1) rather than regenerating from PRNG to avoid confusing existing players whose injected/firewalled devices would suddenly become harder.

**Alternatives considered**: Regenerate device tiers from PRNG during migration — correct values but potentially confuses existing state (a tier-4 target that was hacked now appears different); treat 0 as tier 1 at runtime without migration — possible but obscures save data state.

---

## Decision 8 — Store item SO pattern

**Decision**: Extend `StoreItemSO` and `StoreItem` with a `[Header("Hardware Upgrade")]` section containing `bool HasHardwareUpgrade`, `HardwareStat HardwareStatAffected` (new enum: CPU=0, Internet=1, GPU=2), and `int HardwareTierGranted`. Extend `StoreItemSO.ToModel()` and `StoreService.ApplyEffect()` accordingly.

Create 11 new `StoreItemSO` asset files in `Assets/Resources/StoreItems/`:
- CPU: tiers 2–5 (4 items)
- Internet: tiers 2–5 (4 items)
- GPU: tiers 1–3 (3 items)

**Rationale**: Follows the existing pattern of `HasToolUnlock`/`HasCurrencyUnlock` boolean guards — one per effect type. `StoreController` needs no changes (it renders all SOs in Resources/StoreItems/). The `HardwareTierGranted` field replaces, rather than increments, the player's tier (FR-004/005/006: "purchasing a higher tier replaces the current tier").

---

## Decision 9 — TerminalController integration

**Decision**: In `TerminalController.OnSubmit`, replace `GameManager.Instance.GetCommandLatency()` with: parse the verb from input, look up the command in `CommandParser`, call `command.GetLatency(args)`. `GameManager.CommandLatencyService` is a public accessor (like `LocationService`).

**Rationale**: Commands already know their own latency context. Centralising the lookup in TerminalController requires no changes to any other caller. GameManager already exposes service accessors as public properties (LocationService, CommandParser); CommandLatencyService follows the same pattern.

---

## Decision 10 — Test strategy

**Decision**: EditMode unit tests in `CommandLatencyServiceTests.cs` cover:
- Each command's latency formula at base/mid/max player tiers against tier-1 and tier-5 targets
- Clamp behaviour (floor at 0.05f, cap per command)
- 20× ratio verification for crack and inject
- FR-021: tier-1 target returns identical time to no-target baseline
- StoreService hardware tier application (added to `StoreServiceTests.cs`)

No PlayMode tests added for the latency visual (progress bar already tested in feature 004 via TerminalControllerTests).

**Rationale**: All latency math is pure functions on `Player` and `Device` data with no Unity dependencies — EditMode is sufficient. The visual progress bar's correctness is an existing concern covered by manual UX gate (same gate used in feature 004).
