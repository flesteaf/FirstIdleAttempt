# Feature Specification: Hacking Idle Game — Core Game Loop

**Feature Branch**: `001-hacking-idle-core`
**Created**: 2026-04-13
**Status**: Draft
**Input**: User description: "This is an incremental/idle game with hacking theme"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - First Network Infiltration (Priority: P1)

A new player opens the game and immediately engages with the core hacking loop.
They scan for nearby networks, assess security levels, crack into a target network,
scan connected devices, and inject their first miner to start earning passive income.

**Why this priority**: This is the foundational game loop. Without a working
scan → crack → inject cycle, no other feature can be experienced. It is the
first independently deliverable milestone.

**Independent Test**: Start a fresh game, run `scan`, select a network, crack
its security, scan for IPs, inject a miner, and confirm passive income starts
accumulating.

**Acceptance Scenarios**:

1. **Given** a new game session, **When** the player runs `scan`, **Then** a
   list of nearby networks is displayed with their SSIDs
2. **Given** a discovered network, **When** the player runs `scan network {SSID}`,
   **Then** the security level and available device IPs are shown
3. **Given** a secured network, **When** the player runs the correct
   `crack {TYPE} {SSID}` for that security level, **Then** access is granted
4. **Given** network access, **When** the player runs `scan ip {IP}`, **Then**
   firewall status and open ports of that device are shown
5. **Given** a reachable device, **When** the player runs `inject miner`, **Then**
   the device begins generating virtual currency passively over time
6. **Given** an active miner, **When** the player checks their balance, **Then**
   accumulated earnings from all miners are reflected correctly

---

### User Story 2 - Upgrade and Expand (Priority: P2)

After earning virtual currency, the player visits the store to purchase PC
component upgrades (improving processing power) and software upgrades (unlocking
new commands or improving existing ones).

**Why this priority**: Progression and the upgrade loop are what make the idle
genre engaging. Without this, the game has no sense of advancement.

**Independent Test**: Accumulate currency via US1, open the store, purchase an
item, and confirm the upgrade's effect is active (new command unlocked or mining
speed improved).

**Acceptance Scenarios**:

1. **Given** a player with sufficient virtual currency, **When** they open the
   store, **Then** available PC components and software items are listed with
   prices and descriptions
2. **Given** an affordable store item, **When** the player purchases it, **Then**
   currency is deducted and the upgrade is applied immediately
3. **Given** a software upgrade that unlocks a new command, **When** the player
   uses that command after purchase, **Then** it executes successfully
4. **Given** a PC component upgrade improving mining speed, **When** active
   miners tick, **Then** income rate is visibly higher than before the upgrade

---

### User Story 3 - Accept and Complete Contracts (Priority: P3)

The player browses available contracts (DDOS an IP, obtain a file, facilitate
an attack for another party), selects one matching their capabilities, completes
the required steps, and receives a bonus reward.

**Why this priority**: Contracts provide active engagement goals and bonus
rewards on top of idle income, giving players a reason to return and interact.

**Independent Test**: Select a DDOS contract, complete the attack sequence, and
confirm the contract reward is credited to the player's balance.

**Acceptance Scenarios**:

1. **Given** a player with an active bot injection, **When** they view contracts,
   **Then** a list with objectives and rewards is shown
2. **Given** a selected contract, **When** the player completes all required
   objectives, **Then** the contract is marked complete and the reward credited
3. **Given** a contract requiring capabilities the player lacks, **When** they
   attempt to accept it, **Then** a clear message explains the missing prerequisite

---

### Edge Cases

- What happens when `scan` is run with no networks discoverable at the current location?
- What happens if the player uses the wrong crack tool for a network's security level?
- What happens if a player tries to inject malware on a device with an active firewall?
- What is the upper limit on simultaneously infected devices?
- What happens when the player returns after a long absence and offline income has
  accumulated for an extended period?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Players MUST be able to run `scan` to discover nearby networks;
  a second `scan` without parameters MUST move them to a new location with new networks
- **FR-002**: Players MUST be able to run `scan network {SSID}` to reveal the
  security level and connected device IPs of a discovered network
- **FR-003**: Players MUST be able to run `scan ip {IP}` and `scan mac {MAC}`
  to determine a device's firewall status and available ports
- **FR-004**: Players MUST be able to crack network security using
  `crack WEP {SSID}`, `crack WPA {SSID}`, or `crack WPA2 {SSID}`, each
  requiring the corresponding unlocked tool
- **FR-005**: Players MUST be able to run `show networks` to see all discovered
  networks including already-hacked ones
- **FR-006**: Players MUST be able to run `show ips` to see all infected IPs
  and the malware type on each
- **FR-007**: Players MUST be able to `inject miner` on a reachable device to
  begin passive virtual currency accumulation
- **FR-008**: Players MUST be able to `inject bot` to convert a device into a
  bot, unlocking attack contracts; bot scripting (automated attack sequences) is
  out of scope for MVP
- **FR-009**: Players MUST be able to `inject spammer` to unlock spam contracts
  that generate additional passive income
- **FR-010**: Players MUST be able to `inject ransomware` to encrypt a device
  and display a virtual ransom demand; ransom payment receipt, device recovery,
  and demand expiry are out of scope for MVP
- **FR-011**: Players MUST be able to disable and re-enable a device's firewall
  using `firewall` commands before injecting malware
- **FR-012**: The store MUST list available PC components and software upgrades
  with current prices and descriptions
- **FR-013**: Players MUST be able to purchase store items with earned virtual
  currency; items MUST take effect immediately upon purchase
- **FR-014**: The game MUST offer contracts of at least three types: DDOS
  attacks, file retrieval, and facilitated attacks
- **FR-015**: Contract availability MUST be gated by the player's current
  capabilities (e.g., bot injection required before attack contracts appear)
- **FR-017**: Idle income from miners and spammers MUST accrue indefinitely
  while the game is closed, calculated from the last recorded timestamp when
  the player next opens the game
- **FR-016**: The game MUST start with a single virtual currency (bitcoin); additional
  currencies (altcoins) MUST become available as the player unlocks progression
  milestones through store upgrades, with each currency offering distinct income
  rates and use cases

### Key Entities

- **Network**: A discoverable Wi-Fi network with an SSID, security level
  (open/WEP/WPA/WPA2), and a set of connected device IPs; belongs to a location
- **Device**: A network-connected machine with an IP and MAC address, firewall
  status, open ports, and an optional active malware payload
- **Malware**: A payload injected into a device; types: miner (passive income),
  bot (automation/contracts), spammer (spam contracts), ransomware (one-time payment)
- **Player**: Holds virtual currency balance, unlocked tools/commands, and
  purchased upgrades
- **Store Item**: A purchasable upgrade — either a PC component (improves hardware
  stats) or software (unlocks or enhances commands)
- **Contract**: A task with defined objectives, required capabilities, and a
  currency reward on completion
- **Location**: A virtual area containing a set of discoverable networks; the
  player moves by running `scan` without parameters a second time

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new player can discover their first network, crack it, and inject
  a miner within 5 minutes of starting the game without external guidance
- **SC-002**: Passive income from injected miners is visible to the player within
  30 seconds of a successful injection
- **SC-003**: At least 3 store upgrades are available and purchasable by the time
  a player completes their first network infiltration
- **SC-004**: Players can complete a contract from acceptance to reward in a
  single uninterrupted play session
- **SC-005**: The game remains fully interactive with no perceptible lag while
  managing 10 or more simultaneously infected devices
- **SC-006**: 90% of first-time players can identify their next available action
  at any point in the early game without external prompting

## Assumptions

- The game is single-player; no multiplayer or social features are in scope
- The target platform is PC (desktop), operated via keyboard input
- All virtual currencies are fictional; no real-money transactions are in scope
- The player begins with basic `scan` capability only; all other tools are
  unlocked through store purchases
- Using the wrong crack tool for a network's security level fails the attempt
  with an informative error message
- A device's firewall must be disabled before malware can be injected on it;
  on open networks (SecurityLevel.None) the firewall condition is waived
- Bot scripting (automated attack sequences) is a late-game feature, out of
  scope for the initial MVP
- Ransomware in MVP is limited to the inject action only; ransom payment
  receipt, device recovery, and demand expiry are out of scope
- Idle income (miners, spammers) accrues indefinitely while the game is closed
  and is applied in full when the player next opens the game
- The game launches with bitcoin as the sole currency; altcoins unlock
  progressively through store upgrade milestones
- The maximum number of simultaneously infected devices per session is 50
