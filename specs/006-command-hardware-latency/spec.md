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

The player can purchase CPU upgrades from the shop — moving from a basic single-core laptop up through dual-core, quad-core, workstation-class, and finally a server rack. Higher CPU tiers directly reduce the time taken by commands that are compute-intensive: cracking passwords, injecting malware, and disabling firewalls. The shop item description clearly states which command categories it affects.

**Why this priority**: CPU upgrades are the primary progression driver for offensive commands. Players need to feel that buying a CPU makes cracking and injecting meaningfully faster. This story is the first place where spending currency pays off in reduced command wait times.

**Independent Test**: Purchase tier-2 CPU (dual-core). Run `crack` on an identical WPA2 network before and after the purchase. The time with tier-2 CPU must be measurably shorter. Run `scan` — the time must be unchanged (scan is not CPU-bound).

**Acceptance Scenarios**:

1. **Given** the player buys the "Dual-Core CPU" upgrade, **When** they run `crack` on any network, **Then** the crack command completes faster than it did at tier 1, with the progress bar visibly shorter.
2. **Given** the player has tier-3 CPU, **When** they run `inject`, **Then** inject completes faster than at tier 2 for the same target device.
3. **Given** the player has any CPU tier, **When** they run `scan` (area or IP), **Then** the scan time is identical regardless of CPU tier — scan is not CPU-bound.
4. **Given** the player reaches tier-5 CPU (server rack), **When** they run `crack` on a WEP network, **Then** the crack is near-instant (below 0.1 seconds effective time).

---

### User Story 3 — Player upgrades internet connection to speed up network-bound commands (Priority: P2)

The player can purchase internet connection upgrades: from a slow DSL line through cable, fibre lite, full fibre, and finally a dedicated line. Higher bandwidth tiers reduce the time for all commands that involve transmitting data over the network: area scan, device scan, file listing, file copy, firewall modification, and malware injection. The shop description clearly states which commands benefit.

**Why this priority**: Internet upgrades complement CPU upgrades and give players a second meaningful progression axis. Since most commands have a network component, internet upgrades feel broadly useful rather than niche — they create a satisfying "everything got faster" moment.

**Independent Test**: Purchase tier-2 internet (Cable). Run `scan` on an identical location before and after. Time must be shorter after upgrade. Run `crack` — time must be unchanged (crack is not network-bound).

**Acceptance Scenarios**:

1. **Given** the player buys the "Cable Internet" upgrade, **When** they run `scan` on any location, **Then** the scan completes faster than it did at tier 1.
2. **Given** the player has tier-4 internet, **When** they run `copy` on a file, **Then** copy completes faster than at tier 3 for the same file and target.
3. **Given** the player has any internet tier, **When** they run `crack`, **Then** crack time is identical regardless of internet tier — cracking is local CPU/GPU work.
4. **Given** both a CPU upgrade and an internet upgrade are owned, **When** `inject` is run (a mixed command), **Then** both contribute independently to reducing the total time.

---

### User Story 4 — Player purchases a GPU to dramatically accelerate password cracking (Priority: P3)

The player can buy a GPU from the shop. Unlike CPU or internet upgrades, the GPU only accelerates `crack`. Its effect is dramatic — a tier-2 GPU with a base CPU cuts crack time by more than any single CPU tier jump would. A tier-3 high-end compute GPU makes cracking feel almost instantaneous even on WPA2 networks. The shop item is clearly labelled as affecting cracking only.

**Why this priority**: GPU is a specialised, high-value purchase for players who crack frequently. It creates a meaningful strategic choice (broad upgrade vs. specialised) and adds depth to the shop's progression tree.

**Independent Test**: Purchase tier-1 GPU. Run `crack` on a WPA2 network immediately before and after. The time reduction after purchase must be larger than the reduction from buying one CPU tier.

**Acceptance Scenarios**:

1. **Given** the player has no GPU and tier-1 CPU, **When** they purchase the tier-1 GPU and run `crack`, **Then** the crack time is shorter than with tier-2 CPU and no GPU.
2. **Given** the player has the tier-3 GPU (compute cluster), **When** they crack any security level, **Then** crack time approaches the near-instant threshold.
3. **Given** the player has any GPU tier, **When** they run `scan`, `inject`, `copy`, or any non-crack command, **Then** the time for those commands is identical to having no GPU.

---

### User Story 5 — Target device hardware adds authentic resistance to commands (Priority: P3)

Each discovered device has two hardware properties: a CPU tier and a bandwidth tier. These are generated when the device is created and are visible after `scan ip`. A device with a high CPU tier takes longer to inject malware into and longer to disable its firewall — its security systems respond faster and are harder to overwhelm. A device with high bandwidth transfers data faster, which shortens `ls` and `copy` but slightly increases `scan ip` time (more network complexity). The device's hardware stats feel like genuine character — some targets are tougher than others.

**Why this priority**: This story makes individual targets feel meaningfully different and creates strategic depth — a high-CPU device is a harder but potentially more rewarding target. It also reuses the player's hardware investment: a high-CPU player can take on high-CPU targets that would be impractical with base hardware.

**Independent Test**: Find two devices — one with CPU tier 1 and one with CPU tier 4 (visible via `scan ip`). Run `inject` on both with identical player hardware. The tier-4 device must take measurably longer to inject.

**Acceptance Scenarios**:

1. **Given** a target device has CPU tier 4, **When** the player runs `inject` compared to CPU tier 1, **Then** the inject on the tier-4 device takes longer, reflected by a longer progress bar.
2. **Given** a target device has bandwidth tier 5, **When** the player runs `copy` on a file from it, **Then** the copy completes faster than from a tier-1 bandwidth device (high-bandwidth device transfers data quickly).
3. **Given** a target device has bandwidth tier 5, **When** the player runs `scan ip` on it, **Then** it takes slightly longer than a tier-1 device (higher network complexity adds scan overhead).
4. **Given** the player runs `scan ip`, **When** they view the output, **Then** the device's CPU tier and bandwidth tier are shown alongside its IP, MAC, and firewall status.

---

### Edge Cases

- What happens when a command's effective time drops to zero or below due to upgrades? → Effective time is clamped to a minimum floor (e.g., 0.05 seconds) so the progress bar always appears briefly — "instant" is only achievable via CommandSpeedUpgrade reaching 1.0.
- What happens when the player tries to run two commands back to back while the progress bar is running? → The second command is silently dropped; the terminal remains blocked until the current command finishes (existing behaviour).
- What does `scan ip` show for a device that has not yet been scanned? → CPU and bandwidth tiers are hidden until after the first successful `scan ip` or `scan mac` on that device.
- What happens if the player has maximum CPU and internet but a target device has very high CPU? → Target device CPU can only add up to a defined maximum overhead — it cannot extend the time beyond a cap so that well-upgraded players are never blocked indefinitely.
- What happens when `copy` is run on a very large file with tier-1 internet? → Copy time is capped at a defined maximum (e.g., 15 seconds) to prevent frustratingly long waits at low tiers.
- How does device hardware interact with network security level on `crack`? → Network security level sets the base crack time; `crack` is not affected by target device properties (cracking happens locally, attacking the captured handshake).

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Each command MUST have a defined base execution time (in seconds) determined by its operation category; `show`, `help`, `forget`, and `move` MUST be instant (no progress bar).
- **FR-002**: The effective execution time for any command MUST be calculated from: base time, player CPU tier modifier, player internet tier modifier, player GPU tier modifier, target device CPU tier modifier, and target device bandwidth tier modifier — applying only the modifiers relevant to that command's category (see FR-008 through FR-013).
- **FR-003**: Effective time MUST be clamped to a minimum floor of 0.05 seconds; it MUST also be clamped to a per-command maximum (defined per command in design data) to prevent unbounded waits.
- **FR-004**: The player's CPU hardware tier MUST be a persistent stat with 5 tiers (1 = base, 5 = server rack); purchasing a higher tier replaces the current tier.
- **FR-005**: The player's internet connection tier MUST be a persistent stat with 5 tiers (1 = DSL, 5 = dedicated line); purchasing a higher tier replaces the current tier.
- **FR-006**: The player's GPU tier MUST be a persistent stat with 3 tiers (0 = none, 1 = basic gaming, 2 = workstation, 3 = compute cluster); purchasing a higher tier replaces the current tier.
- **FR-007**: Each generated `Device` MUST be assigned a CPU tier (1–5) and a bandwidth tier (1–5) at creation time; these values MUST persist in the save file.
- **FR-008**: `crack` execution time MUST be reduced by higher player CPU tier and player GPU tier; it MUST NOT be affected by player internet tier or target device properties.
- **FR-009**: `scan` (area and IP/MAC variants) execution time MUST be reduced by higher player internet tier; area scan MUST NOT be affected by CPU or GPU; IP/MAC scan MAY add a small overhead for high target bandwidth tier.
- **FR-010**: `inject` execution time MUST be reduced by higher player internet tier and player CPU tier; it MUST be increased by higher target device CPU tier and target device bandwidth tier.
- **FR-011**: `firewall` execution time MUST be reduced by higher player internet tier; it MUST be increased by higher target device CPU tier.
- **FR-012**: `ls` execution time MUST be reduced by higher player internet tier and higher target device bandwidth tier.
- **FR-013**: `copy` execution time MUST be reduced by higher player internet tier and higher target device bandwidth tier; it MUST also scale with the file size of the file being copied.
- **FR-014**: The shop MUST offer CPU upgrade items for tiers 2–5, internet upgrade items for tiers 2–5, and GPU items for tiers 1–3; each item description MUST state which commands it affects.
- **FR-015**: Purchasing a hardware upgrade item MUST immediately apply to all subsequent commands in the same session — no restart required.
- **FR-016**: `scan ip` (or `scan mac`) output MUST display the target device's CPU tier and bandwidth tier once scanned.
- **FR-017**: The existing progress bar (ASCII `[████░░░░] NN%`) MUST reflect the actual effective time for each command — the bar duration must match the command's effective latency.
- **FR-018**: Player hardware tiers (CPU, internet, GPU) MUST be persisted in the save file and restored on reload.
- **FR-019**: The combined effect of all three upgrade axes at maximum tier MUST reduce effective command time by exactly 20× relative to base tier on a maximum-tier target device; the tier coefficients for each axis MUST be chosen so that no single axis alone provides more than 10× of that 20× reduction.
- **FR-020**: `Player.CommandSpeedUpgrade` MUST be removed from the player model; `GameManager.GetCommandLatency()` MUST be replaced by a per-command latency calculation that uses the new hardware tier stats.
- **FR-021**: A tier-1 target device ("old technology") MUST NOT contribute any overhead modifier to any command — it represents the neutral baseline. Only devices above tier 1 add resistance modifiers.

### Key Entities

- **PlayerHardware**: CPU tier (1–5), internet tier (1–5), GPU tier (0–3). Part of the player's persistent state. Drives the player-side modifiers in all latency calculations.
- **CommandProfile**: Named set of (base seconds, max seconds, list of modifiers that apply). One profile per command verb. Defined in designer data (ScriptableObject or constant table), not hardcoded per-call.
- **DeviceHardware** (new properties on `Device`): CPU tier (1–5), bandwidth tier (1–5). Generated at device creation; persisted in save; displayed after device scan.
- **HardwareShopItem** (new shop item subtype): Tier number, hardware stat affected (CPU / Internet / GPU), speed upgrade contribution. Purchased via the existing store; applies its effect to the matching player hardware stat.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A fully-upgraded player (CPU tier 5, internet tier 5, GPU tier 3) executes every command at least 20× faster than a base-tier player (CPU 1, internet 1, GPU 0) when the target device is at maximum tier — this is the design ceiling for player progression.
- **SC-002**: Against a tier-1 target device ("old technology"), the 20× ratio may not be fully observable as an absolute time difference because the base player's time is already shorter on easy targets — the ratio holds in the math but old targets require less hardware investment to feel fast.
- **SC-003**: The 20× total speedup is distributed across all three upgrade axes (CPU, internet, GPU) so that no single axis alone delivers more than 10× — players are incentivised to invest in all three.
- **SC-004**: With base-tier hardware, the difference in duration between the fastest non-instant command (`ls`) and the slowest (`crack` on WPA2) is at least 3× — players clearly feel the distinction between command types.
- **SC-005**: A tier-5 target device CPU (vs. tier-1) adds no more than 100% overhead to `inject` time, ensuring high-end targets remain beatable without requiring tier-5 player hardware.
- **SC-006**: All hardware tier values and device hardware properties survive a save/reload cycle with zero data loss.
- **SC-007**: No command exceeds its defined maximum execution time regardless of target device hardware.

## Assumptions

- `Player.CommandSpeedUpgrade` (introduced in feature 004) is **fully removed** and replaced by the explicit CPU tier, internet tier, and GPU tier stats. Any code that read `CommandSpeedUpgrade` must be migrated to use the new per-stat system; there is no backward-compatibility shim.
- Device CPU and bandwidth tiers are generated procedurally by the `LocationService` at device creation time, using the same PRNG seed as other device properties, so they are deterministic and reproducible.
- All hardware shop items use Bitcoin as their purchase currency, consistent with existing shop items.
- Security level (`None`, `WEP`, `WPA`, `WPA2`) on a `Network` continues to determine `crack` base time; the target device does not further modify crack time.
- File size for `copy` is the `DeviceFile.SizeBytes` field already present on `DeviceFile`; no new file model changes are needed.
- `move` may eventually have a small latency representing connection routing, but for this feature it remains instant — its latency story belongs to a future travel/routing feature.
- The 5-tier and 3-tier hardware scales are fixed; adding more tiers is a future expansion outside this feature's scope.

## Clarifications

*(None — all design decisions resolved via assumptions above.)*
