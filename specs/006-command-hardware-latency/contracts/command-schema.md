# Command Contracts: Hardware Latency

**Feature**: `specs/006-command-hardware-latency` | **Date**: 2026-04-25

All latency values are effective seconds after applying player hardware and target device modifiers. `minFloor = 0.05f`. Instant commands have no progress bar.

---

## crack {WEP|WPA|WPA2} {SSID}

**Category**: Compute-bound (local, no network)

**Latency formula**:
```
effectiveTime = Clamp(
    baseCrackTime[secLevel] / (cpu_speedup[playerCpuTier] × gpu_speedup[playerGpuTier]),
    0.05, maxCrackCap[secLevel]
)
```

**Base times and caps**:
| Security | Base (s) | Cap (s) |
|----------|---------|--------|
| WEP | 3.0 | 10.0 |
| WPA | 6.0 | 20.0 |
| WPA2 | 10.0 | 30.0 |

**Player modifiers**: CPU tier (1–5) and GPU tier (0–3) only.

**Target modifiers**: None — cracking operates on the captured handshake locally.

**Instant?**: No.

**Pre-execution validation** (unchanged from existing): tool ownership, security level match, network not already hacked.

**Progress bar visible**: Yes.

**Example** — base player (CPU1, GPU0) cracking WPA2: `10.0 / (1.0 × 1.0) = 10.0s`.
**Example** — max player (CPU5, GPU3) cracking WPA2: `10.0 / (5.0 × 4.0) = 0.5s` → above 0.05 floor.

---

## scan

**Category**: Network-bound

### scan (area)

**Latency formula**:
```
effectiveTime = Clamp(1.5 / internet_speedup[playerInternetTier], 0.05, 8.0)
```

**Player modifiers**: Internet tier only.

**Target modifiers**: None (no specific target for area scan).

**Instant?**: No.

### scan ip {IP} / scan mac {MAC}

**Latency formula**:
```
effectiveTime = Clamp(
    2.0 × target_bw_scan_overhead[targetBwTier] / internet_speedup[playerInternetTier],
    0.05, 8.0
)
```

**Player modifiers**: Internet tier only.

**Target modifiers**: Target device bandwidth tier adds a small overhead (high-complexity network is slower to enumerate). Tier-1 overhead = 1.0× (neutral, FR-021).

**Output changes** (FR-016): After `IsScanned = true`, output includes:
```
IP:        192.168.X.Y
MAC:       AA:BB:CC:DD:EE:FF
CPU Tier:  N/5
BW Tier:   N/5
Firewall:  Active | Disabled
Ports:     22, 80, 443
```

**Instant?**: No.

---

## inject {IP}

**Category**: Mixed — network + compute

**Latency formula**:
```
effectiveTime = Clamp(
    4.0 × targetCpuResistance[targetCpuTier] × targetBwResistance[targetBwTier]
        / (cpu_speedup[playerCpuTier] × internet_speedup[playerInternetTier]),
    0.05, 15.0
)
```

**Player modifiers**: CPU tier and Internet tier (both reduce time).

**Target modifiers**:
- Target CPU tier: higher CPU = better defences = more resistance.
- Target BW tier: higher bandwidth = more traffic complexity = slight extra resistance.
- Tier-1 target CPU and BW both = 1.0× (neutral, FR-021).

**Max target overhead**: `target_cpu_resistance[5] = 2.0` (100% overhead max per SC-005). Combined with `target_bw_resistance[5] = 1.30`, the tier-5+tier-5 target multiplies base time by up to 2.0 × 1.30 = 2.6×. This extra time is fully overcome by CPU5 × Internet5 = 20× speedup.

**Pre-execution validation** (unchanged): device found, firewall disabled or open network.

**Instant?**: No.

---

## firewall {IP}

**Category**: Network-bound (with target CPU resistance)

**Latency formula**:
```
effectiveTime = Clamp(
    2.5 × targetCpuResistance[targetCpuTier] / internet_speedup[playerInternetTier],
    0.05, 12.0
)
```

**Player modifiers**: Internet tier only.

**Target modifiers**: Target CPU tier (defensive systems respond faster on powerful hardware). Tier-1 = 1.0× (neutral).

**Instant?**: No.

---

## ls {IP}

**Category**: Network-bound (target bandwidth helps)

**Latency formula**:
```
effectiveTime = Clamp(
    0.5 / (internet_speedup[playerInternetTier] × targetBwSpeedup[targetBwTier]),
    0.05, 8.0
)
```

**Player modifiers**: Internet tier (reduces time).

**Target modifiers**: Target bandwidth tier speeds up file listing — faster devices share file metadata more quickly. Tier-1 BW = 1.0× (neutral).

**Instant?**: No.

---

## copy {IP} {file}

**Category**: Network-bound (target bandwidth helps; scales with file size)

**Latency formula**:
```
fileScaling  = 1.0 + FileSizeBytes / 10_000_000 × 2.0
adjustedBase = 3.0 × fileScaling
effectiveTime = Clamp(
    adjustedBase / (internet_speedup[playerInternetTier] × targetBwSpeedup[targetBwTier]),
    0.05, 15.0
)
```

**Player modifiers**: Internet tier.

**Target modifiers**: Target bandwidth tier.

**File size**: Uses `DeviceFile.SizeBytes` (already present on `DeviceFile`). The `10_000_000` reference size (10 MB) and `2.0` scale factor are named constants in `CommandLatencyService`.

**Example**: 1 MB file at base player, tier-1 target: `(3.0 × (1 + 1/10 × 2)) / (1.0 × 1.0) = 3.6s`.
**Example**: 20 MB file, base player, tier-1 target: `(3.0 × (1 + 2 × 2)) = 15s` → hits 15s cap.

**Instant?**: No.

---

## show / help / forget / move

**Category**: Instant — local operations with no hardware dependency.

**GetLatency**: Returns `0f` (default interface method, no override needed).

**Progress bar**: Never shown.

---

## Coefficient reference tables

All table indices start at 1 (tiers are 1-based). GPU table is 0-based.

```
cpu_speedup           = { 1=1.0f, 2=1.5f, 3=2.5f, 4=3.5f, 5=5.0f }
internet_speedup      = { 1=1.0f, 2=1.5f, 3=2.3f, 4=3.2f, 5=4.0f }
gpu_speedup           = { 0=1.0f, 1=1.6f, 2=2.8f, 3=4.0f }
targetCpuResistance   = { 1=1.0f, 2=1.1f, 3=1.3f, 4=1.6f, 5=2.0f }
targetBwSpeedup       = { 1=1.0f, 2=1.2f, 3=1.5f, 4=1.8f, 5=2.2f }
targetBwResistance    = { 1=1.0f, 2=1.05f, 3=1.12f, 4=1.20f, 5=1.30f }
targetBwScanOverhead  = { 1=1.0f, 2=1.05f, 3=1.10f, 4=1.15f, 5=1.20f }
```
