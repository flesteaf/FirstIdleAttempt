# Feature Specification: Network Discovery Income — Direct Inject by IP/SSID

**Feature Branch**: `002-network-discovery-income`
**Created**: 2026-04-13
**Status**: Draft
**Input**: User request: "ensure that the basic income generation is possible through
network discovery, using 'scan' command, and infecting the discovered devices in those
networks, using 'inject miner' and specifying the ip and network of the device to infect."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Direct Injection Without Pre-Targeting (Priority: P1)

A player discovers networks with `scan`, sees device IPs with `scan network {SSID}`,
and wants to inject malware on a specific device immediately by supplying the IP and
SSID directly in the inject command — without first running `scan ip {IP}` to set a
session target. The player can go from network discovery to income generation in fewer
commands.

**Why this priority**: This is the primary UX improvement requested. It collapses the
required step count (eliminating the mandatory `scan ip` targeting step) and makes the
core income loop more discoverable for new players. All other behaviour is unchanged.

**Independent Test**: Start a fresh game, run `scan`, note an SSID, run
`scan network {SSID}` to see device IPs, crack the network (or use an open network),
then run `inject miner {IP} {SSID}` directly. Confirm income starts accruing without
ever running `scan ip`.

**Acceptance Scenarios**:

1. **Given** a player who has cracked a network, **When** they run
   `inject miner {IP} {SSID}` with a valid IP and SSID, **Then** malware is installed
   and income begins accruing.
2. **Given** a player with an open (SecurityLevel.None) network, **When** they run
   `inject miner {IP} {SSID}`, **Then** injection succeeds without a prior `scan ip`.
3. **Given** a network that has NOT been cracked (IsHacked=false, SecurityLevel!=None),
   **When** the player runs `inject miner {IP} {SSID}`, **Then** an error is returned
   stating the network is not accessible.
4. **Given** a valid IP/SSID pair where the device has an active firewall, **When** the
   player runs `inject miner {IP} {SSID}`, **Then** an error states the firewall must
   be disabled first.
5. **Given** a device already infected, **When** the player runs
   `inject miner {IP} {SSID}`, **Then** an error states the device is already infected.
6. **Given** an SSID that does not exist in the current location, **When** the player
   runs `inject miner {IP} {SSID}`, **Then** an error states the network was not found.
7. **Given** a valid SSID but an IP not present on that network, **When** the player
   runs `inject miner {IP} {SSID}`, **Then** an error states the device was not found
   on that network.
8. **Given** a player who runs `inject miner` with no arguments and has a TargetedDevice
   set, **Then** the existing (legacy) targeting behaviour executes normally — full
   backward compatibility.
9. **Given** a player who runs `inject bot {IP} {SSID}` (or spammer/ransomware),
   **When** all preconditions are met, **Then** the correct malware type is installed
   and the appropriate output is shown.

---

### Edge Cases

- What happens if `inject miner {IP}` is run with only one trailing argument (IP but no SSID)?
- What happens if the SSID belongs to a previous location, not the current one?
- What happens if an IP appears on multiple networks in the same location? (Only the specified SSID is searched.)
- What happens if SSID contains spaces? (Out of scope — SSIDs are single tokens in the generator.)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Players MUST be able to run `inject {type} {IP} {SSID}` where IP and SSID
  are optional; when provided, the command MUST resolve the target device from the
  current location's network data rather than from `Player.TargetedDevice`.
- **FR-002**: When IP and SSID are provided, the command MUST validate that the SSID
  exists in the current location (not a previous location).
- **FR-003**: The command MUST validate network accessibility: `network.IsHacked == true`
  OR `network.SecurityLevel == SecurityLevel.None`. All other security states are blocked.
- **FR-004**: When IP and SSID are provided, the command MUST validate that the IP
  exists as a device on the resolved network.
- **FR-005**: All existing inject preconditions (firewall check, already-infected check,
  software ownership check) MUST apply identically whether the device is resolved from
  arguments or from `Player.TargetedDevice`.
- **FR-006**: When no IP/SSID arguments are provided, the command MUST behave exactly as
  in Schema v1.0.0 — no behaviour change (full backward compatibility).
- **FR-007**: All four malware types (miner, bot, spammer, ransomware) MUST support the
  new optional IP/SSID argument form.
- **FR-008**: If exactly one of IP or SSID is provided (but not both), the command MUST
  return a usage error explaining the correct syntax.

### Key Entities

No new entities are introduced. This feature extends `InjectCommand` to resolve a
`Device` + `Network` pair from `LocationService` rather than from `Player.TargetedDevice`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A player can go from `scan` → `scan network {SSID}` → `crack` →
  `inject miner {IP} {SSID}` with four commands and zero `scan ip` calls.
- **SC-002**: All existing `InjectCommandTests` continue to pass without modification
  (backward compatibility verified by the unchanged test suite).
- **SC-003**: Nine new test cases (one per acceptance scenario) all pass.
- **SC-004**: An error message is produced for every invalid input variant
  with text matching `contracts/command-schema.md` v1.1.0 exactly.
- **SC-005**: `inject miner {IP} {SSID}` on a valid open-network device succeeds even
  when `Player.TargetedDevice` is null.

## Assumptions

- SSIDs are single whitespace-free tokens; multi-word SSIDs are not in scope.
- Only the current location is searched; cross-location injection is not supported.
- `Player.TargetedDevice` is NOT updated by the direct-inject path; targeting state
  is changed only by `scan ip` / `scan mac`.
- Network accessibility rule: `IsHacked == true OR SecurityLevel == None`.
  A scanned but un-cracked network is NOT accessible.
