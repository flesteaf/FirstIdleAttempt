# Feature Specification: Location, Network Persistence & Movement

**Feature Branch**: `feature/004-location-network-persistence`
**Created**: 2026-04-18
**Status**: Draft

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Reload preserves all discovered networks and infected IPs (Priority: P1)

After restarting the game, the player can immediately see all previously discovered networks and all infected IPs — income continues seamlessly without having to re-scan anything.

**Why this priority**: This is the core bug. Without persistence of the view layer, all other features are undermined. Income accrues correctly but the player cannot verify or manage their infection portfolio.

**Independent Test**: Start a fresh session, scan networks, infect IPs, quit the game, relaunch, run `show networks` and `show ips` — all previously discovered data must appear.

**Acceptance Scenarios**:

1. **Given** the player has discovered 3 networks and infected 2 IPs across them, **When** the game is closed and reopened, **Then** `show networks` displays all 3 networks and `show ips` displays both infected IPs with their income rates.
2. **Given** the player is at location A with infected IPs, **When** the game reloads, **Then** offline income is already applied AND `show ips` lists the same devices that were generating income.
3. **Given** the player has visited 2 locations, **When** the game reloads, **Then** `show networks` shows networks from both locations (grouped by location).

---

### User Story 2 — Player can move between locations explicitly (Priority: P2)

The player can use a `move` command to discover a brand-new location or revisit a previously known one by name.

**Why this priority**: Movement was a hidden side-effect of running `scan` twice. Making `move` the explicit, sole navigation command — and removing the double-scan idiom from `scan` — makes the game's navigation model unambiguous and removes an accidental location change risk.

**Independent Test**: Run `move` with no args to reach a new location; run `move node_77` to return to the starting location. Both must update what `scan` reveals.

**Acceptance Scenarios**:

1. **Given** the player is at any location, **When** they run `move` with no arguments, **Then** they arrive at a newly generated location; the terminal prints a confirmation line with the new location's name, and the HUD header updates to show the new location name. No networks are displayed — the player must run `scan` to discover them.
2. **Given** the player has previously visited "node_77", **When** they run `move node_77`, **Then** they return to that location; the terminal confirms with the location name and the HUD header updates. The player must run `scan` to see networks.
3. **Given** the player runs `move unknown_location`, **When** that location name is not in the player's known locations, **Then** an error is displayed: "Location 'unknown_location' not found. Use `move` without arguments to discover a new location." HUD is not updated.
4. **Given** the player is already at location X, **When** they run `move X` (same location), **Then** the terminal confirms they are already there; no movement occurs and HUD is unchanged.

---

### User Story 3 — Player can forget networks or IPs to manage their portfolio (Priority: P3)

The player can remove a discovered network (and all its infected IPs) or a single infected IP from their records. Forgotten entries stop contributing to income.

**Why this priority**: As the player discovers many locations, they may want to clean up low-value infections or abandon compromised networks. Without forget, the infected list grows unboundedly.

**Independent Test**: Infect a device, confirm income contribution in `show ips`, run `forget ip <IP>`, confirm income drops accordingly and `show ips` no longer lists the device.

**Acceptance Scenarios**:

1. **Given** an infected IP is visible in `show ips`, **When** the player runs `forget ip <IP>`, **Then** the device is removed from all location records, income from that device stops immediately, and `show ips` no longer lists it.
2. **Given** a network with 3 infected IPs is visible in `show networks`, **When** the player runs `forget network <SSID>`, **Then** the network and all 3 IPs are removed, their combined income stops, and neither network nor its IPs appear in any `show` output.
3. **Given** the player runs `forget network <SSID>` for a network that has no infected IPs, **Then** the network is removed from `show networks` without any income change.
4. **Given** the player runs `forget ip <IP>` for an IP not in any location records, **Then** an error is displayed: "IP not found in any known location."
5. **Given** a network with the same SSID exists at two different locations, **When** the player runs `forget network <SSID>`, **Then** the terminal displays a numbered list of matching locations and waits for the player to enter a number to confirm which one to forget.

---

### User Story 4 — `inject` guides the player through type and network selection interactively (Priority: P2)

When the player runs `inject` without specifying all arguments, the terminal guides them step by step: first choosing a malware type from their unlocked options, then choosing a target network from the current location. This removes the need to memorize exact syntax.

**Why this priority**: The existing `inject` command requires exact syntax with type and network arguments. With the location system growing, players need a discoverable way to inject without knowing every SSID by heart. This also introduces a reusable interactive selection pattern for the terminal.

**Independent Test**: Run `inject` with no arguments — terminal shows arrow-navigable numbered malware type list with "Cancel" as the last option; navigate with arrow keys and press Enter — terminal shows the network selection list; select a network — injection proceeds as if the full command had been typed.

**Acceptance Scenarios**:

1. **Given** the player runs `inject` with no arguments, **When** the command is processed, **Then** the terminal displays an arrow-navigable numbered list of available malware types (unlocked only) with "Cancel" as the last option.
2. **Given** the player has selected a type (step 1), **When** no network was specified in the original command, **Then** the terminal displays an arrow-navigable numbered list of accessible networks at the current location with "Cancel" as the last option.
3. **Given** the player has selected both type and network interactively, **When** Enter is pressed to confirm, **Then** injection proceeds with the same precondition checks and output as the full `inject <type> <SSID>` command.
4. **Given** the player runs `inject miner` (type provided, no network), **When** the command is processed, **Then** the terminal skips type selection and goes directly to the network selection list.
5. **Given** the player runs `inject miner HomeNet_100` (full args), **When** the command is processed, **Then** no interactive selection occurs — injection proceeds immediately as before.
6. **Given** the player is at a location with no accessible networks during interactive selection, **When** the network list would be shown, **Then** an error is displayed: "No accessible networks at current location. Crack a network first."

---

### User Story 6 — `firewall`, `ls`, and `copy` accept explicit IP or guide through interactive device selection (Priority: P2)

The `firewall`, `ls`, and `copy` commands no longer rely on an implicitly targeted device. The player can either supply a device IP directly in the command or, if omitted, be guided through an arrow-navigable numbered list of discovered devices at the current location.

**Why this priority**: Removing `TargetedDevice`/`TargetedNetwork` is a breaking change for three commands. Making these commands self-sufficient — either via explicit argument or interactive selection — is required before any of the targeting-dependent flows can be tested end to end.

**Independent Test**: Run `firewall disable` with no IP — terminal shows arrow-navigable device list; select a device — firewall is disabled on that device exactly as if `firewall disable <IP>` had been typed.

**Acceptance Scenarios**:

1. **Given** the player runs `firewall disable <IP>`, **When** the command is processed, **Then** the firewall on the specified device is toggled without any interactive step (explicit path, no selection list shown).
2. **Given** the player runs `firewall disable` with no IP, **When** the command is processed, **Then** the terminal displays an arrow-navigable numbered list of discovered devices at the current location with "Cancel" as the last option.
3. **Given** the player runs `ls <IP>`, **When** the command is processed, **Then** the files on the specified device are listed without any interactive step.
4. **Given** the player runs `ls` with no IP, **When** the command is processed, **Then** the terminal displays an arrow-navigable numbered list of discovered devices at the current location; selecting a device lists its files.
5. **Given** the player runs `copy <filename> <IP>`, **When** the command is processed, **Then** the file is copied from the specified device without any interactive step.
6. **Given** the player runs `copy <filename>` with no IP, **When** the command is processed, **Then** the terminal displays an arrow-navigable numbered list of discovered devices at the current location; selecting a device copies the file from it.
7. **Given** `firewall`, `ls`, or `copy` enters device-selection mode and no devices have been discovered at the current location, **When** the list would be shown, **Then** an error is returned immediately: "No devices discovered at current location. Use `scan` first."

---

### User Story 5 — Player can view all known locations (Priority: P3)

The player can see all locations they have discovered, how many networks each has, and how many IPs are infected there.

**Why this priority**: Once multiple locations exist, the player needs an overview to manage their operations effectively.

**Independent Test**: Visit 3 locations, run `show locations` — all 3 appear with network counts and infection counts.

**Acceptance Scenarios**:

1. **Given** the player has visited 2 locations, **When** they run `show locations`, **Then** each location is listed with its name, network count, and number of infected IPs.
2. **Given** the player is at location X, **When** they run `show locations`, **Then** the current location is visually marked (e.g., with `>` prefix).

---

### Edge Cases

- What happens when the player runs `move` and all generated locations have been visited? → System always generates a new procedural location (there is no cap).
- What happens when a location has all its networks forgotten? → Location remains in known locations list with 0 networks; player can still `move` there.
- What happens if the same SSID name appears at two locations? → `forget network <SSID>` displays a numbered list of matching locations for the player to select from; `forget network <SSID> at <location>` skips the list. `show networks` groups all output by location to disambiguate.
- What is the income impact when `forget ip` is called mid-session? → Forget applies from the current tick; any offline income already credited is kept.
- What happens when `scan` is run but no current location is available? → `scan` returns an error message; the player must use `move` to establish a location first.
- What does `show locations` display when the player has never visited any location? → Column headers only, consistent with `show networks` and `show ips` empty-state behaviour.
- What does `show networks` display when no locations have been visited yet? → Column headers only (e.g., `NETWORK  LOCATION  SECURITY  STATUS`) with no data rows beneath.
- What does `show ips` display when no devices are infected? → Column headers only (e.g., `IP  NETWORK  TYPE  INCOME/s`) with no data rows beneath.
- What happens when the player enters a number out of range during interactive selection? → Treated as invalid; list stays visible with the highlight unchanged, waiting for a valid key.
- What happens when `inject` (or any) interactive selection is in progress and the player types free text? → Selection flow is cancelled immediately; typed text is discarded; terminal returns to normal input.
- How does the player cancel an interactive selection? → Three ways: (1) select the "Cancel" option from the list using arrow keys + Enter, (2) type its number directly, or (3) type any free text.
- What happens when `inject` reaches the network selection step but no networks are accessible? → Error returned immediately; selection flow ends.
- What happens when `firewall`, `ls`, or `copy` is used without an IP argument and there are no discovered devices at the current location? → Error returned immediately; no selection list shown.
- What happens when `scan ip <IP>` is called after `TargetedDevice` is removed? → `scan ip <IP>` still shows device details (firewall, ports) but no longer sets a targeted device; the player uses explicit IP arguments or interactive selection in subsequent commands.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST persist all discovered locations, their networks, and device states across game sessions.
- **FR-002**: `show networks` MUST display networks from all known locations after reload, grouped by location.
- **FR-003**: `show ips` MUST display all infected IPs across all known locations after reload.
- **FR-004**: Offline income calculation MUST remain consistent regardless of `forget` operations performed in subsequent sessions.
- **FR-005**: System MUST assign each location a unique, human-readable name (e.g., "node_77") generated deterministically from its seed at creation time.
- **FR-006**: `move` with no arguments MUST navigate the player to a newly generated, previously unvisited location; the terminal MUST print one confirmation line with the new location's name; the HUD header MUST update to the new location name; no networks are shown automatically.
- **FR-007**: `move <location-name>` MUST navigate the player to a previously visited location by its name; the terminal MUST print one confirmation line; the HUD header MUST update; no networks are shown automatically.
- **FR-008**: `move <unknown-name>` MUST return an error and leave the current location and HUD unchanged.
- **FR-009**: `forget network <SSID>` MUST remove the network and all its devices from all location records, stopping their income contributions.
- **FR-010**: When `forget network <SSID>` matches multiple locations, the terminal MUST present a numbered list of matching locations for the player to select from; `forget network <SSID> at <location>` MUST also continue to work as a direct non-interactive alternative.
- **FR-011**: `forget ip <IP>` MUST remove that device from its location record and stop its income contribution.
- **FR-012**: `show locations` MUST list all known locations with name, network count, and infected IP count.
- **FR-013**: The current location MUST be tracked and restored on game reload so the player returns to where they left off.
- **FR-014**: Income service MUST aggregate income from infected devices across ALL known locations, not only the currently active location.
- **FR-015**: `scan` MUST NOT trigger location movement under any circumstances; `move` is the sole location navigation mechanism.
- **FR-016**: `scan` MUST return an error message if no current location is available (e.g., on a fresh game before the first `move` has been issued).
- **FR-017**: When `forget network <SSID>` matches networks at multiple locations, the terminal MUST display a numbered list of those locations and await a number selection rather than returning an error.
- **FR-018**: `inject` with no arguments MUST display a numbered list of the player's unlocked malware types and await a number selection before proceeding.
- **FR-019**: After malware type is determined (by argument or selection), if no network is specified, `inject` MUST display a numbered list of accessible networks at the current location and await a number selection.
- **FR-020**: Type selection (step 1) MUST always precede network selection (step 2) in the interactive inject flow.
- **FR-021**: Interactive selection lists MUST be navigable with arrow keys (up/down); the currently highlighted option is visually indicated. Pressing Enter (or equivalent confirm key) selects the highlighted option.
- **FR-021b**: Every interactive selection list MUST include "Cancel" as the last numbered option. Selecting it aborts the flow and returns to normal command input.
- **FR-021c**: Typing any free-text input (non-number, non-arrow) while a selection is active MUST cancel the current selection flow immediately and return to the normal command prompt. The typed text is discarded (not processed as a new command).
- **FR-022**: Full-argument `inject <type> <SSID>` MUST continue to work exactly as before — no interactive steps triggered.
- **FR-023**: `show networks` MUST always display column headers, even when no networks have been discovered (empty data set).
- **FR-024**: `show ips` MUST always display column headers, even when no devices are infected (empty data set).
- **FR-025**: The `TargetedDevice` and `TargetedNetwork` session-state fields MUST be removed from the player model entirely; no command may rely on implicit device targeting.
- **FR-026**: `firewall`, `ls`, and `copy` MUST each accept an explicit device IP as an argument (e.g., `firewall disable <IP>`, `ls <IP>`, `copy <filename> <IP>`); if no IP is provided, the command MUST display a numbered list of discovered devices at the current location for the player to select from, following the same interactive selection pattern as `inject` (FR-018–FR-021).
- **FR-027**: If `firewall`, `ls`, or `copy` enters device-selection mode and the current location has no discovered devices, the command MUST return an error immediately without prompting.

### Key Entities

- **Location**: Unique name (e.g., "node_77"), seed for procedural generation, list of discovered networks, visit timestamp.
- **Known Locations** (formerly referred to as "KnownLocations"): The full set of all locations the player has visited — persisted in the save file. Not a separate class; implemented as the in-memory cache (`_locationCache`) maintained by the location service, backed by the locations list in the save file.
- **CurrentLocationName**: The name of the player's active location — persisted so reload returns the player to the correct place.
- **Network**: Belongs to a specific location; SSID, security level, hack status, device list.
- **Device**: Belongs to a network; IP, MAC, firewall status, active malware if infected.
- ~~**TargetedDevice / TargetedNetwork**~~: **Removed.** These session-state fields are eliminated from the player model. Commands that previously relied on implicit targeting (`firewall`, `ls`, `copy`) must now accept an explicit IP argument or guide the player through interactive device selection.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After game reload, `show ips` lists all infected IPs that were visible before closing — zero data loss across sessions.
- **SC-002**: The `move` command is the sole mechanism for changing location; the double-scan idiom is removed from `scan`. Running `scan` never changes the player's current location.
- **SC-003**: Income shown in the HUD immediately after reload matches the income rate that was active before closing (within one tick tolerance).
- **SC-004**: `forget network` removes a network and all its IPs in one command; the income contribution drops to zero within the same game tick.
- **SC-005**: `show locations` displays all visited locations with accurate counts after a save/reload cycle.

## Assumptions

- Location names are generated deterministically from the location seed using a formula that yields "node_77" for seed 1 (e.g., `"node_" + (seed * 77 % 1000)`).
- The first/default location is "node_77" — the starting location for every new game.
- `show networks` (no extra args) changes from showing only current-location networks to showing networks across all known locations, grouped by location name.
- The income service currently only ticks on the current location; fixing it to tick on all known locations is part of this feature.
- SSID names can collide across locations (both procedurally generated using the same template pool); the `at <location>` qualifier resolves ambiguity.
- The double-scan-to-move behavior is REMOVED from `ScanCommand`; `move` is the sole location navigation mechanism. `scan` discovers networks within the current location only and never changes location.
- `scan` returns an error if no current location is available (e.g., first launch of a fresh game before `move` has been issued).
- `TargetedDevice` and `TargetedNetwork` are removed from the player model; `move` no longer needs to clear them. Commands that relied on them (`firewall`, `ls`, `copy`) now require explicit IP arguments or use interactive device selection.
- Save schema is versioned; existing saves (version 1) receive a migration that back-fills location names from seeds.

## Clarifications

### Session 2026-04-18

- Q: Should double-scanning (`scan` → `scan` again) still trigger location movement after `move` is introduced? → A: No — the double-scan-to-move behavior is removed from `ScanCommand`. `move` is the sole location navigation mechanism. Additionally, `scan` must return an error if no current location is available.
- Q: When `forget network <SSID>` is ambiguous (same SSID at two locations), what should the response look like? → A: Display a numbered list of matching locations; player selects by number. This interactive numbered-selection pattern also applies to `inject`: if no type is provided, show a numbered list of unlocked types first; if no network is provided after type resolution, show a numbered list of accessible networks at the current location. Type selection always precedes network selection.
- Q: What should `show networks` and `show ips` display on a fresh game with no data? → A: Column headers only (e.g., `NETWORK  LOCATION  SECURITY  STATUS`), no data rows beneath.
- Q: After `move` delivers the player to a new location, should networks be auto-scanned or must the player run `scan` manually? → A: `move` navigates only — outputs one confirmation line with the new location name and updates the HUD header. No networks are displayed. Player runs `scan` separately to discover networks.
- Q: When `move` is called, what happens to `TargetedDevice` and `TargetedNetwork`? → A: Both are removed entirely from the player model. Commands that relied on them (`firewall`, `ls`, `copy`) must now accept an explicit IP argument or use interactive numbered-list device selection, following the same pattern as `inject`.
- Q: Can the player cancel or escape from a mid-flight interactive selection? → A: Three mechanisms: (1) arrow-navigate to the "Cancel" option (always last in the list) and press Enter, (2) type the "Cancel" option's number directly, or (3) type any free text — this immediately cancels the flow and discards the input. Selection lists are arrow-key navigable; the highlighted option is visually indicated; Enter confirms.
- Q: Do `firewall`, `ls`, and `copy` need dedicated acceptance scenarios? → A: Yes — User Story 6 added covering explicit-IP path and interactive device-selection path for all three commands.
