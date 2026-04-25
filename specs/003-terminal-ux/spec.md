# Feature Specification: Terminal UX Enhancements

**Feature Branch**: `003-terminal-ux`
**Created**: 2026-04-14
**Status**: Draft
**Input**: User description: "I want to have a true terminal behaviour. If end of the visible is reached, the text scrolls itself down, to always have the latest message in view, autocomplete for commands, etc."

## Overview

The in-game terminal is the primary interaction surface for the hacking idle game. Currently it lacks core terminal conventions that players expect: output does not automatically scroll to show the latest line, and there is no way to complete partially-typed commands without remembering exact syntax. This feature brings the terminal in line with real-world terminal emulator behaviour, reducing friction and making the game feel more authentic.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Auto-Scroll to Latest Output (Priority: P1)

When the player runs a command that produces multiple lines of output, the terminal view automatically scrolls down so the most recent line is always visible. The player never has to manually scroll after issuing a command.

**Why this priority**: Without auto-scroll, long command output (e.g. `scan`, `ls`) pushes new lines off the bottom of the visible area. The player loses context and the core gameplay loop feels broken. This is the single biggest usability gap.

**Independent Test**: Open the terminal, run `ls` on a device with many files so that output exceeds the visible height. Verify the last line of output is visible without any manual scroll interaction, and that the input field remains accessible at the bottom.

**Acceptance Scenarios**:

1. **Given** the terminal has output that fits within the visible area, **When** the player submits a command whose response fits in the remaining space, **Then** all output is visible and no scrolling occurs.
2. **Given** the terminal has accumulated enough output to exceed the visible height, **When** a new command response is appended, **Then** the view automatically scrolls to the bottom so the latest response line is in view.
3. **Given** the player has manually scrolled up to review earlier output, **When** a new command response arrives, **Then** the view scrolls back to the bottom automatically (new output takes precedence over manual scroll position).
4. **Given** the terminal receives multiple rapid output lines (e.g. a tick event and a command response simultaneously), **Then** the scroll position ends at the very last line without flickering mid-scroll.

---

### User Story 2 - Command Autocomplete (Priority: P2)

The player can press Tab while typing a command to complete or suggest matching command names. If only one match exists the command name is completed in full. If multiple matches exist, all candidates are displayed in the terminal so the player can see options.

**Why this priority**: The game has a growing set of commands with specific syntax. Autocomplete reduces typing errors, helps players discover available commands, and reinforces the authentic terminal feel.

**Independent Test**: With the terminal input field focused, type `sc` and press Tab. Verify `scan` is completed automatically. Then clear the field, type `i` and press Tab, verify candidates (`inject`) are displayed or completed.

**Acceptance Scenarios**:

1. **Given** the player has typed a prefix that matches exactly one registered command, **When** the player presses Tab, **Then** the input field is filled with the full command name and the cursor is positioned after it.
2. **Given** the player has typed a prefix that matches multiple registered commands, **When** the player presses Tab, **Then** the terminal displays all matching command names as a suggestion line (e.g. `> inject  ls`) and the input field is unchanged.
3. **Given** the player has typed a prefix that matches no registered command, **When** the player presses Tab, **Then** nothing changes and no suggestion line is shown.
4. **Given** the player has typed a complete command followed by a space and a partial argument, **When** the player presses Tab, **Then** autocomplete attempts to complete the argument against known values (device IPs, file names) if applicable; otherwise no change.
5. **Given** the input field is empty, **When** the player presses Tab, **Then** all registered commands are listed as suggestions.

---

### User Story 3 - Command History Navigation (Priority: P3)

The player can press the Up and Down arrow keys in the input field to cycle through previously submitted commands, allowing rapid re-entry of recent commands without retyping.

**Why this priority**: History navigation is a standard terminal convention. It is less critical than scroll and autocomplete but rounds out the "true terminal" feel and saves time during repetitive gameplay sequences.

**Independent Test**: Submit three different commands in sequence. Press Up once and verify the most recent command appears in the input field. Press Up again to verify the second-most-recent appears. Press Down to verify navigation back toward the most recent entry.

**Acceptance Scenarios**:

1. **Given** at least one command has been submitted, **When** the player presses Up, **Then** the input field shows the most recently submitted command.
2. **Given** the player has pressed Up multiple times, **When** the player presses Down, **Then** the input field moves forward through history toward the most recent entry.
3. **Given** the player is at the oldest history entry and presses Up again, **Then** the oldest entry remains in the field (no wrap-around).
4. **Given** the player is at the most recent history entry and presses Down again, **Then** the input field clears (returns to blank state ready for new input).
5. **Given** the player partially modifies a history entry and then submits it, **Then** the modified version is saved as the newest history entry.

---

### Edge Cases

- What happens when the terminal output is cleared mid-scroll? The scroll position should reset to the bottom.
- How does autocomplete behave when the input field contains leading or trailing whitespace? Whitespace is trimmed before matching.
- What is the maximum number of history entries retained? A reasonable cap (e.g. 50 entries) prevents unbounded memory growth; oldest entries are dropped when the cap is exceeded.
- What happens if autocomplete is triggered while a command is executing (output is streaming)? Autocomplete still operates on the static command registry; no conflict.
- What happens if the player uses a controller or on-screen keyboard without a Tab key? Tab-based autocomplete is keyboard-only; players without a Tab key rely on typing full commands (no regression from current behaviour).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The terminal view MUST automatically scroll to the bottom whenever new output is appended, so the latest line is always visible without player intervention.
- **FR-002**: The auto-scroll MUST also trigger when output is generated by background processes (e.g. income tick notifications), not only from direct player commands.
- **FR-003**: When the player presses Tab in the command input field, the system MUST attempt to complete the typed prefix against the list of registered commands.
- **FR-004**: If exactly one command matches the typed prefix, the system MUST replace the current input text with the full command name.
- **FR-005**: If multiple commands match the typed prefix, the system MUST display all candidates as a non-interactive suggestion line in the terminal output area.
- **FR-006**: If no commands match the typed prefix, Tab MUST have no visible effect on input or output.
- **FR-007**: The system MUST maintain a command history list of all commands submitted during the current session.
- **FR-008**: Pressing the Up arrow key MUST cycle backward through command history and populate the input field with the selected entry.
- **FR-009**: Pressing the Down arrow key MUST cycle forward through command history; reaching the newest entry and pressing Down again MUST clear the input field.
- **FR-010**: Command history MUST be capped at 50 entries per session; entries beyond the cap are discarded oldest-first.
- **FR-011**: All three behaviours (auto-scroll, autocomplete, history) MUST work independently — disabling or breaking one MUST NOT affect the others.

### Key Entities

- **CommandRegistry**: The ordered collection of command names available for autocomplete lookup. Read-only at runtime.
- **CommandHistory**: The session-bound list of submitted command strings, capped at 50 entries, with a current-index pointer for Up/Down navigation.
- **TerminalOutputBuffer**: The sequence of output lines displayed in the terminal view, used as the scroll target source of truth.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of command responses (both player-triggered and background) cause the terminal view to scroll to the bottom without any player action; verifiable by automated or manual test with output exceeding visible height.
- **SC-002**: Tab completion resolves a unique prefix to the full command name in a single keypress with no additional interaction required.
- **SC-003**: Players can navigate to any of the last 10 submitted commands using only Up/Down arrow keys within 10 keypresses.
- **SC-004**: All three terminal UX features introduce no measurable input lag — the player's keystroke produces a visible response within the same frame it is received.
- **SC-005**: The terminal UX features do not regress any existing command functionality; all existing edit-mode and play-mode tests continue to pass after implementation.

## Assumptions

- The game targets desktop keyboard input; Tab and arrow-key bindings are safe to claim without conflicting with Unity UI navigation defaults (navigation on the input field will be overridden as needed).
- The terminal input is a single-line text field; multi-line input editing is out of scope.
- Autocomplete covers only the command name (first token); argument completion for sub-commands (e.g. device IPs after `scan ip`) is a P3 nice-to-have within User Story 2 Acceptance Scenario 4 and may be deferred.
- Command history is session-only and does not persist across game sessions (save/load).
- The scroll area is already a scrollable container in the existing UI; this feature adds scroll-to-bottom logic, not a new scrollable region.
- On-screen / gamepad input is out of scope; no virtual Tab or arrow-key equivalent is required.
