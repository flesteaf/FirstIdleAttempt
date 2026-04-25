# Feature Specification: Command Hardware Latency

**Feature Branch**: `feature/006-command-hardware-latency`
**Created**: 2026-04-25
**Status**: Draft

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Commands take realistic, variable time to execute (Priority: P1)

Each terminal command now takes a different amount of time based on what it actually does. A network scan takes longer than viewing a list. Cracking a WPA2 network takes longer than cracking WEP. The player can see a progress bar counting down. When their hardware is all at base tier, times feel challenging but not punishing. The flat 1-second universal delay is replaced by per-command timing.

**Why this priority**: This is the foundation for every other story in this feature. Without per-command base times, hardware upgrades have nothing to modify. It also immediately makes the game feel more authentic — every command has a reason for its speed.

**Independent Test**: Start a fresh session with all hardware at tier 1. Run `crack` on a WPA2 network and observe it takes noticeably longer than `scan`. Run `ls` and observe it takes less time than `copy`. Run `show` and observe it is instant. All times are shown via the existing progress bar.

**Acceptance Scenarios**:

1. **Given** the player has base-tier hardware, **When** they run `crack` on a WPA2 network, **Then** the progress bar runs for longer than when cracking a WEP network, reflecting the greater computational effort.
2. **Given** the player runs `copy` with a large file vs. a small file, **When** both commands complete, **Then** the large-file copy took proportionally longer.
3. **Given** the player runs `show`, `help`, `forget`, or `move`, **When** the command executes, **Then** no progress bar is shown — these are instant local operations.
4. **Given** any non-instant command, **When** it executes, **Then** the progress bar updates smoothly and disappears when the command completes.

---

### User Story 2 — Player upgrades CPU to speed up compute-bound commands (Priority: P2)

The player can purchase CPU upgrades from the shop — moving from a basic single-core laptop up through dual-core, quad-core, workstation-class, and finally a server rack. Higher CPU tiers directly reduce the time taken by commands that are compute-intensive: cracking passwords. The shop item description clearly states which command categories it affects.

**Why this priority**: CPU upgrades are the primary progression driver for cracking. Players need to feel that buying a CPU makes cracking meaningfully faster. This story is the first place where spending currency pays off in reduced command wait times.

**Independent Test**: Purchase tier-2 CPU (dual-core). Run `crack` on an identical WPA2 network before and after the purchase. The time with tier-2 CPU must be measurably shorter. Run `scan` — the time must be unchanged (scan is not CPU-bound).

**Acceptance Scenarios**:

1. **Given** the player buys the "Dual-Core CPU" upgrade, **When** they run `crack` on any network, **Then** the crack command completes faster than it did at tier 1, with the progress bar visibly shorter.
2. **Given** the player has tier-3 CPU, **When** they run `crack` on a WPA2 network, **Then** crack completes faster than with tier-2 CPU — each CPU tier provides a proportional, measurable reduction.
3. **Given** the player has any CPU tier, **When** they run `scan` (area or IP), **Then** the scan time is identical regardless of CPU tier — scan is not CPU-bound.
4. **Given** the player reaches tier-5 CPU (server rack), **When** they run `crack` on a WEP network, **Then** the crack is near-instant (below 0.1 seconds effective time).

---

### User Story 3 — Player upgrades internet connection to speed up network-bound commands (Priority: P2)

The player can purchase internet connection upgrades: from a slow DSL line through cable, fibre lite, full fibre, and finally a dedicated line. Higher bandwidth tiers reduce the time for all commands that involve transmitting data over the network: area scan, device scan, file listing, file copy, firewall modification, and malware injection. The shop description clearly states which commands benefit.

**Why this priority**: Bandwidth upgrades complement CPU upgrades and give players a second meaningful progression axis. Since most commands have a network component, bandwidth upgrades feel broadly useful rather than niche — they create a satisfying "everything got faster" moment.

**Independent Test**: Purchase tier-2 internet (Cable). Run `scan` on an identical location before and after. Time must be shorter after upgrade. Run `crack` — time must be unchanged (crack is not network-bound).

**Acceptance Scenarios**:

1. **Given** the player buys the "Cable Internet" upgrade, **When** they run `scan` on any location, **Then** the scan completes faster than it did at tier 1.
2. **Given** the player has bandwidth tier 4, **When** they run `copy` on a file, **Then** copy completes faster than at tier 3 for the same file and target (assuming the target is not the bottleneck).
3. **Given** the player has any bandwidth tier, **When** they run `crack`, **Then** crack time is identical regardless of bandwidth tier — cracking is local CPU/GPU work.
4. **Given** the player has bandwidth tier 2 and copies a file from a tier-4 bandwidth target, **When** they upgrade to bandwidth tier 3, **Then** copy is faster; after upgrading to bandwidth tier 5 (above the target's tier 4), no further reduction is observed — the target's connection is now the bottleneck, and additional player bandwidth cannot overcome it.

---

### User Story 4 — Player purchases a GPU to dramatically accelerate password cracking (Priority: P3)

The player can buy a GPU from the shop. Unlike CPU or bandwidth upgrades, the GPU only accelerates `crack`. Its effect is dramatic — a tier-2 GPU with a base CPU cuts crack time by more than any single CPU tier jump would. A tier-3 high-end compute GPU makes cracking feel almost instantaneous even on WPA2 networks. The shop item is clearly labelled as affecting cracking only.

**Why this priority**: GPU is a specialised, high-value purchase for players who crack frequently. It creates a meaningful strategic choice (broad upgrade vs. specialised) and adds depth to the shop's progression tree.

**Independent Test**: Purchase tier-1 GPU. Run `crack` on a WPA2 network immediately before and after. The time reduction after purchase must be larger than the reduction from buying one CPU tier.

**Acceptance Scenarios**:

1. **Given** the player has no GPU and tier-1 CPU, **When** they purchase the tier-1 GPU and run `crack`, **Then** the crack time is shorter than with tier-2 CPU and no GPU.
2. **Given** the player has the tier-3 GPU (compute cluster), **When** they crack any security level, **Then** crack time approaches the near-instant threshold.
3. **Given** the player has any GPU tier, **When** they run `scan`, `inject`, `copy`, or any non-crack command, **Then** the time for those commands is identical to having no GPU.

---

### User Story 5 — Target device hardware shapes network speed and payload effectiveness (Priority: P3)

Each discovered device has two hardware properties: a CPU tier and a bandwidth tier. These are generated when the device is created and are visible after `scan ip`. The target's bandwidth tier is one end of the network connection: data-transfer commands are limited by whichever connection is slower — the player's or the target's. A low-bandwidth target caps transfer speed even if the player has a fast line; a high-bandwidth target lets the player use their full connection speed. The target's CPU tier does not affect how long commands take — it determines what the device can do once a payload is running: a high-CPU target running an injected miner generates more value per second than a low-CPU one.

**Why this priority**: Target hardware makes individual devices feel meaningfully different. High-bandwidth targets are more efficient to interact with — copies and listings are faster when the target keeps up. High-CPU targets are more valuable as payload destinations. Both properties create strategic choices: upgrading player bandwidth only pays off against high-bandwidth targets, and seeking high-CPU devices matters for mining yield.

**Independent Test**: Find two devices — one with bandwidth tier 1 and one with bandwidth tier 4 (visible via `scan ip`). With player bandwidth at tier 2, run `copy` on both. The tier-4 device's copy must complete faster (the tier-1 target is the bottleneck; the tier-4 target is not). Then inject a miner into both a CPU tier 1 and a CPU tier 5 device — after waiting, the tier-5 device's miner must produce measurably more output than the tier-1 device's, while the injection time must be identical for both (same player and target bandwidth tiers assumed).

**Acceptance Scenarios**:

1. **Given** the player has bandwidth tier 1 and a target has bandwidth tier 5, **When** the player runs `copy`, **Then** copy completes at tier-1 speed — the player's connection is the bottleneck regardless of target bandwidth.
2. **Given** the player has bandwidth tier 3 and two targets with bandwidth tier 1 and tier 5, **When** the player runs `copy` on each, **Then** the tier-5 target's copy completes faster (player is not the bottleneck), while the tier-1 target's copy is slower (target is the bottleneck).
3. **Given** two targets with CPU tier 1 and CPU tier 5 have both had a miner injected, **When** the player waits for miner output, **Then** the tier-5 target generates measurably more mining yield per second; the injection execution time was identical for both given equal bandwidth conditions.
4. **Given** the player runs `scan ip`, **When** they view the output, **Then** the device's CPU tier and bandwidth tier are shown alongside its IP, MAC, and firewall status.

---

### Edge Cases

- What happens when a command's effective time drops to zero or below due to upgrades? → Effective time is clamped to a minimum floor (e.g., 0.05 seconds) so the progress bar always appears briefly — no command reaches true zero duration through hardware tier upgrades alone.
- What happens when the player tries to run two commands back to back while the progress bar is running? → The second command is silently dropped; the terminal remains blocked until the current command finishes (existing behaviour).
- What does `scan ip` show for a device that has not yet been scanned? → CPU and bandwidth tiers are hidden until after the first successful `scan ip` or `scan mac` on that device.
- What happens if the player upgrades their bandwidth beyond the target's tier? → For network commands, effective speed is capped at the target's bandwidth tier — additional player bandwidth provides no benefit against that target. The player must seek higher-bandwidth targets to make full use of a top-tier connection.
- What happens when `copy` is run on a very large file with tier-1 bandwidth on both sides? → Copy time is capped at a defined maximum (e.g., 15 seconds) to prevent frustratingly long waits at low tiers.
- How does device hardware interact with network security level on `crack`? → Network security level sets the base crack time; `crack` is not affected by target device properties (cracking happens locally, attacking the captured handshake).

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Each command MUST have a defined base execution time (in seconds) determined by its operation category; `show`, `help`, `forget`, and `move` MUST be instant (no progress bar).
- **FR-002**: The effective execution time for any command MUST be calculated from: base time, player CPU tier modifier, player bandwidth tier modifier, player GPU tier modifier, and (for network-transfer commands) an effective network speed derived from `min(player_bandwidth_tier, target_bandwidth_tier)` — applying only the modifiers relevant to that command's category (see FR-008 through FR-013). Target device CPU tier MUST NOT affect execution time of any command.
- **FR-003**: Effective time MUST be clamped to a minimum floor of 0.05 seconds; it MUST also be clamped to a per-command maximum (defined per command in design data) to prevent unbounded waits.
- **FR-004**: The player's CPU hardware tier MUST be a persistent stat with 5 tiers (1 = base, 5 = server rack); purchasing a higher tier replaces the current tier.
- **FR-005**: The player's bandwidth tier MUST be a persistent stat with 5 tiers (1 = DSL, 5 = dedicated line), representing connection throughput; purchasing a higher tier replaces the current tier.
- **FR-006**: The player's GPU tier MUST be a persistent stat with 3 tiers (0 = none, 1 = basic gaming, 2 = workstation, 3 = compute cluster); purchasing a higher tier replaces the current tier.
- **FR-007**: Each generated `Device` MUST be assigned a CPU tier (1–5) and a bandwidth tier (1–5) at creation time; these values MUST persist in the save file.
- **FR-008**: `crack` execution time MUST be reduced by higher player CPU tier and player GPU tier; it MUST NOT be affected by player bandwidth tier or target device properties.
- **FR-009**: Area scan execution time MUST be reduced by higher player bandwidth tier and MUST NOT be affected by CPU, GPU, or target device properties. `scan ip`/`scan mac` execution time MUST be determined by `min(player_bandwidth_tier, target_bandwidth_tier)` — both sides represent connection throughput, and the slower one limits probe exchange speed.
- **FR-010**: `inject` execution time MUST be determined by `min(player_bandwidth_tier, target_bandwidth_tier)` — it represents pushing a payload over the network and is governed solely by the slowest connection involved. Player CPU tier and target device CPU tier MUST NOT affect inject execution time.
- **FR-011**: `firewall` execution time MUST be determined by `min(player_bandwidth_tier, target_bandwidth_tier)`; target device CPU tier MUST NOT affect firewall execution time.
- **FR-012**: `ls` execution time MUST be determined by `min(player_bandwidth_tier, target_bandwidth_tier)` — higher values on either side only help if they are not the bottleneck.
- **FR-013**: `copy` execution time MUST be determined by `min(player_bandwidth_tier, target_bandwidth_tier)` and MUST scale with the file size of the file being copied.
- **FR-014**: The shop MUST offer CPU upgrade items for tiers 2–5, bandwidth upgrade items for tiers 2–5, and GPU items for tiers 1–3; each item description MUST state which commands it affects.
- **FR-015**: Purchasing a hardware upgrade item MUST immediately apply to all subsequent commands in the same session — no restart required.
- **FR-016**: `scan ip` (or `scan mac`) output MUST display the target device's CPU tier and bandwidth tier once scanned.
- **FR-017**: The existing progress bar (ASCII `[████░░░░] NN%`) MUST reflect the actual effective time for each command — the bar duration must match the command's effective latency.
- **FR-018**: Player hardware tiers (CPU, bandwidth, GPU) MUST be persisted in the save file and restored on reload.
- **FR-019**: The combined effect of all three upgrade axes at maximum tier MUST reduce effective command time by exactly 20× relative to base tier on a maximum-tier target device; the tier coefficients for each axis MUST be chosen so that no single axis alone provides more than 10× of that 20× reduction.
- **FR-020**: `Player.CommandSpeedUpgrade` MUST be removed from the player model; `GameManager.GetCommandLatency()` MUST be replaced by a per-command latency calculation that uses the new hardware tier stats.
- **FR-021**: For all network-transfer commands, effective transfer speed MUST be `min(player_bandwidth_tier, target_bandwidth_tier)`; a tier-1 target bandwidth is the lowest bottleneck, capping speed regardless of player bandwidth tier. Target bandwidth MUST NOT be modelled as resistance — both sides represent throughput capacity, and the slower end limits the transfer.
- **FR-022**: The yield or output rate of an injected payload (e.g., a miner's Bitcoin generation rate) MUST be proportional to the target device's CPU tier; target GPU is not separately modelled — CPU tier approximates the device's total compute capacity for this feature. Payload yield MUST NOT be affected by the target's bandwidth tier, player hardware, or injection execution time.
- **FR-023**: `inject` MUST pass the payload's expected yield tier (derived from target CPU tier per FR-022) to the running payload so that the player can observe different output rates on different target devices.

### Key Entities

- **PlayerHardware**: CPU tier (1–5), bandwidth tier (1–5), GPU tier (0–3). Part of the player's persistent state. Drives the player-side modifiers in all latency calculations.
- **CommandProfile**: Named set of (base seconds, max seconds, list of modifiers that apply). One profile per command verb. Defined in designer data (ScriptableObject or constant table), not hardcoded per-call.
- **DeviceHardware** (new properties on `Device`): CPU tier (1–5), bandwidth tier (1–5). Generated at device creation; persisted in save; displayed after device scan.
- **HardwareShopItem** (new shop item subtype): Tier number, hardware stat affected (CPU / Bandwidth / GPU), speed upgrade contribution. Purchased via the existing store; applies its effect to the matching player hardware stat.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A fully-upgraded player (CPU tier 5, bandwidth tier 5, GPU tier 3) executes every command at least 20× faster than a base-tier player (CPU 1, bandwidth 1, GPU 0) when the target device is at maximum tier — this is the design ceiling for player progression.
- **SC-002**: Against a tier-1 bandwidth target, network command speed is capped at tier-1 throughput regardless of player bandwidth tier — upgrading player bandwidth past tier 1 provides no benefit for such targets. The 20× ceiling is only fully observable against tier-5 bandwidth targets, creating a natural incentive to seek high-bandwidth devices.
- **SC-003**: The 20× total speedup is distributed across all three upgrade axes (CPU, bandwidth, GPU) so that no single axis alone delivers more than 10× — players are incentivised to invest in all three.
- **SC-004**: With base-tier hardware, the difference in duration between the fastest non-instant command (`ls`) and the slowest (`crack` on WPA2) is at least 3× — players clearly feel the distinction between command types.
- **SC-005**: All hardware tier values and device hardware properties survive a save/reload cycle with zero data loss.
- **SC-006**: No command exceeds its defined maximum execution time regardless of target device hardware.

## Assumptions

- `Player.CommandSpeedUpgrade` (introduced in feature 004) is **fully removed** and replaced by the explicit CPU tier, bandwidth tier, and GPU tier stats. Any code that read `CommandSpeedUpgrade` must be migrated to use the new per-stat system; there is no backward-compatibility shim.
- Device CPU and bandwidth tiers are generated procedurally by the `LocationService` at device creation time, using the same PRNG seed as other device properties, so they are deterministic and reproducible.
- All hardware shop items use Bitcoin as their purchase currency, consistent with existing shop items.
- Security level (`None`, `WEP`, `WPA`, `WPA2`) on a `Network` continues to determine `crack` base time; the target device does not further modify crack time.
- File size for `copy` is the `DeviceFile.SizeBytes` field already present on `DeviceFile`; no new file model changes are needed.
- `move` may eventually have a small latency representing connection routing, but for this feature it remains instant — its latency story belongs to a future travel/routing feature.
- The 5-tier and 3-tier hardware scales are fixed; adding more tiers is a future expansion outside this feature's scope.

## Clarifications

*(None — all design decisions resolved via assumptions above.)*
