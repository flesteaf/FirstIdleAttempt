# Feature Specification: Game Save Management

**Feature Branch**: `007-game-save-management`  
**Created**: 2026-04-28  
**Status**: Draft  
**Input**: User description: "I need a system command that will allow the user to: handle up to 7 save 'files' (Save, Load, Remove Save), start a new game, setting (like audio level, colour scheme, difficulty) - settings are part of future plans, to not be implemented now -"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Save Game Progress (Priority: P1)

A player wants to preserve their current progress by saving to one of seven numbered slots. They issue a save command with a target slot number and receive confirmation that the save succeeded.

**Why this priority**: Saving is the foundational operation — without it, no other save-management actions have meaning. It also protects players from losing progress due to crashes or accidental exits.

**Independent Test**: Can be fully tested by issuing a save command targeting any slot (1–7) and verifying that slot now contains the player's current state.

**Acceptance Scenarios**:

1. **Given** the player has progress and slot 3 is empty, **When** they save to slot 3, **Then** slot 3 contains the saved state and a success message is displayed.
2. **Given** slot 5 already holds a save, **When** the player saves to slot 5, **Then** the system prompts "Slot 5 is occupied, overwrite? (y/n)" and only overwrites if the player confirms with "y".
3. **Given** the player tries to save to slot 8 (out of range), **When** the command runs, **Then** an error states that valid slots are 1–7.

---

### User Story 2 - Load a Saved Game (Priority: P2)

A player wants to return to a previously saved state. They issue a load command with a slot number and the game restores that snapshot.

**Why this priority**: Loading is the complement of saving; together they deliver the core save-file loop. It directly unblocks the ability to resume play.

**Independent Test**: Can be tested by first saving to a slot, then issuing a load command for that slot and verifying the game state matches what was saved.

**Acceptance Scenarios**:

1. **Given** slot 2 holds a save, **When** the player loads from slot 2, **Then** the game restores the saved state and a success message is displayed.
2. **Given** slot 6 is empty, **When** the player tries to load from slot 6, **Then** an error is displayed indicating the slot is empty.
3. **Given** the player loads a save, **When** the load completes, **Then** the current unsaved progress is replaced by the loaded state.

---

### User Story 3 - Remove a Saved Game (Priority: P3)

A player wants to free up a save slot or discard an unwanted save. They issue a remove command for the target slot.

**Why this priority**: Slot management becomes necessary when all 7 slots fill up or when a player wants a clean slate for a specific slot.

**Independent Test**: Can be tested by saving to a slot, then removing it and verifying the slot appears empty in the save list.

**Acceptance Scenarios**:

1. **Given** slot 4 holds a save, **When** the player removes slot 4, **Then** the slot is cleared and a success message is displayed.
2. **Given** slot 1 is empty, **When** the player removes slot 1, **Then** an error indicates there is nothing to remove.

---

### User Story 4 - Start a New Game (Priority: P4)

A player wants to reset the game to its initial state and begin fresh — independent of any existing saves.

**Why this priority**: New-game functionality is essential for replayability but is independent of save-slot operations; it can be added after the save/load/remove core is stable.

**Independent Test**: Can be tested by starting from a state with progress, issuing the new-game command, and verifying the game returns to the initial default state.

**Acceptance Scenarios**:

1. **Given** the player has active unsaved progress, **When** they start a new game, **Then** the system displays "Unsaved progress will be lost. Continue? (y/n)" and only resets if the player confirms with "y".
2. **Given** the player confirms the new-game prompt, **When** the reset completes, **Then** the game state matches the initial default state identical to a brand-new installation.
3. **Given** the player starts a new game, **When** they check their save slots, **Then** the existing save slots are unchanged (new game does not erase saves).

---

### User Story 5 - View Save Slot Overview (Priority: P5)

A player wants to see which slots are occupied and basic metadata (e.g., date/time saved, progress level) before deciding where to save or load.

**Why this priority**: Without this overview, players cannot make informed decisions about slot selection; it significantly improves usability of the other operations.

**Independent Test**: Can be tested by issuing the list/overview command and verifying it correctly reflects which slots are occupied and which are empty.

**Acceptance Scenarios**:

1. **Given** slots 1 and 3 are occupied and the rest are empty, **When** the player views the overview, **Then** slots 1 and 3 show save metadata and slots 2, 4–7 show as empty.
2. **Given** all 7 slots are empty, **When** the player views the overview, **Then** all slots display as empty with no errors.

---

### Edge Cases

- What happens when a player tries to save to an already-occupied slot? (overwrite silently vs. confirm prompt)
- What happens when the player starts a new game without saving? (immediate reset vs. data-loss warning)
- What happens if the save storage location becomes unavailable (e.g., disk full, read-only)?
- What happens when the player loads a save from a previous game version that is incompatible with the current format?
- What happens if all 7 slots are occupied and the player wants to save again?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow players to save their current game state to any slot numbered 1 through 7.
- **FR-002**: The system MUST allow players to load a previously saved game state from any occupied slot (1–7).
- **FR-003**: The system MUST allow players to remove an existing save from any occupied slot (1–7).
- **FR-004**: The system MUST prevent save, load, and remove operations on slot numbers outside the range 1–7, displaying an error.
- **FR-005**: The system MUST prevent loading from or removing an empty slot, displaying a clear error message.
- **FR-006**: The system MUST display a list of all 7 slots with their occupied/empty status and, for occupied slots, at minimum the date and time the save was created.
- **FR-007**: The system MUST allow a player to start a new game, resetting all progress to the default initial state.
- **FR-008**: Starting a new game MUST NOT erase or modify any existing save slots.
- **FR-009**: All operations (save, load, remove, new game) MUST display a clear success or failure message upon completion.
- **FR-011**: When saving to an already-occupied slot, the system MUST prompt the player to confirm the overwrite before replacing the existing save data.
- **FR-012**: When starting a new game, the system MUST warn the player that unsaved progress will be lost and require explicit confirmation before resetting.
- **FR-013**: Settings management (audio level, colour scheme, difficulty, etc.) is explicitly OUT OF SCOPE for this feature.

### Key Entities

- **Save Slot**: A numbered storage space (1–7) that holds a snapshot of the player's complete game state at a point in time. Attributes: slot number, occupied status, creation timestamp, metadata summary (e.g., progress level or in-game time).
- **Game State**: The complete set of player progress data captured in a save — including hardware owned, finances, levels, and any other persistent game data. Used both as the source for saving and the target for loading.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A player can complete a save, load, or remove operation in under 3 seconds from issuing the command.
- **SC-002**: Saved game data is fully restored after loading — no progress, items, or state are missing or corrupted.
- **SC-003**: All 7 save slots operate independently; loading or removing one slot does not affect data in other slots.
- **SC-004**: A player can identify which slots are occupied and when they were last saved without issuing additional commands.
- **SC-005**: Starting a new game consistently begins from the same default initial state every time, regardless of previous progress.
- **SC-006**: Save data persists across game sessions — data saved in one session is accessible in a later session.

## Assumptions

- Settings management (audio level, colour scheme, difficulty) is out of scope for this feature and will be addressed in a future specification.
- Save operations target a persistent storage location that survives game restarts.
- "New game" resets to a single, well-defined initial state; there is no branching starting configuration.
- The save management feature is exposed as one or more terminal commands within the game's existing command interface, consistent with the current command structure.
- Each save slot stores the complete game state; partial or differential saves are not required.
- No network or cloud sync of save data is required for this feature.
- Save data format compatibility across game versions is a known risk but version migration is out of scope for this initial feature.
