<!--
SYNC IMPACT REPORT
Version change: 1.0.1 → 1.0.2
Modified principles:
  - Dev Workflow — branch naming: HYW-{issue-number} prefix made optional; pattern simplified to feature/{short-description}
Added sections: N/A
Removed sections: N/A
Templates requiring updates: N/A
Follow-up TODOs: None.
-->

# HackYourWay Constitution

## Core Principles

### I. Code Quality

All C# code MUST be clean, readable, and maintainable. Every MonoBehaviour and
system component MUST have one clearly defined responsibility (Single Responsibility
Principle). Magic numbers MUST be replaced with named constants or serialized
inspector fields. Dead code MUST NOT be committed. All public APIs MUST have XML
documentation comments. Deeply nested logic (>3 levels) MUST be extracted into
named methods or helper classes.

**Rationale**: Unity projects accumulate technical debt rapidly when code quality
is not enforced early. Clear ownership and purpose prevent spaghetti MonoBehaviour
hierarchies that become untestable and impossible to extend as the game grows.

### II. Testing Standards

All game logic MUST be testable in isolation via the Unity Test Framework (NUnit,
edit-mode and play-mode). New features MUST be accompanied by unit tests before
the PR is merged. Tests MUST be written and confirmed failing before implementation
begins (Red-Green-Refactor). Integration tests MUST cover all cross-system
interactions (e.g., command parsing → inventory state → UI feedback). Test
coverage MUST NOT regress; any PR that reduces coverage requires explicit written
justification and reviewer sign-off.

**Rationale**: A test-first discipline was established in HYW-12 and MUST be
maintained. Untested hacking mechanics and progression logic lead to balance and
state-management bugs that are expensive to diagnose in later milestones. Unity 6
(6000.4.2f1) supports the Test Runner with edit-mode and play-mode tests, making
enforcement low-friction.

### III. User Experience Consistency

All UI elements MUST follow a unified visual language: consistent font sizes,
color palette, spacing, and interaction patterns across every game screen. New
terminal commands and UI flows MUST match established patterns (command format,
output style, error messages). Player-facing feedback for every action (errors,
confirmations, state changes) MUST be explicit and unambiguous. No feature MUST
ship without a complete end-to-end player journey validated by manual playtesting
documented in the PR description.

**Rationale**: HackYourWay is a terminal-style hacking idle game; UX consistency
is the primary immersion vector. Inconsistent command output formats or UI styles
break the "hacker" theme and create player confusion. Every new command, store
item, or contract MUST feel native to the same game world.

### IV. Performance Requirements

The game MUST maintain a stable 45 FPS on minimum target hardware (PC with
integrated GPU, 4 GB RAM). No single frame MUST exceed 22.22 ms of CPU work
during normal gameplay. Memory allocations in hot paths (Update loops, command
processing, idle tick resolution) MUST be avoided; use object pooling and
pre-allocated collections. The Unity Profiler MUST be run against any feature
that introduces new Update() calls, coroutines, or runtime asset loading, and
Profiler data MUST be attached to the PR before merge.

**Rationale**: Idle games run continuously, often in the background. Performance
regressions degrade experience silently and compound over long play sessions.
Early profiling prevents costly architecture rewrites in later milestones.

## Development Workflow

- All new features MUST be developed on a dedicated feature branch following
  the naming pattern `feature/{short-description}` (e.g. `feature/004-location-network-persistence`).
  An issue-number prefix (e.g. `HYW-{N}`) is optional.
- Every feature branch MUST target `develop`; direct pushes to `develop` or
  `main` are forbidden.
- Pull requests MUST pass all quality gates and the Constitution Check before
  any reviewer approves.
- Code review MUST explicitly confirm compliance with all four Core Principles.
- Playtesting notes covering the affected game loop MUST be included in the
  PR description before the PR is considered ready for review.

## Quality Gates

All of the following gates MUST be green before any PR is merged:

1. **Build Gate**: Project builds in Unity 6 (6000.4.2f1) without errors or warnings.
2. **Test Gate**: All edit-mode and play-mode tests pass in the Unity Test Runner.
3. **Performance Gate**: No new Update() hot paths are introduced without Profiler
   data attached to the PR.
4. **UX Gate**: Affected UI flows are reviewed against the established visual and
   command-output style.
5. **Constitution Check**: Reviewer explicitly confirms in the PR checklist that
   no principle from this constitution is violated.

## Governance

This constitution supersedes all other development practices and personal preferences.
Amendments require:

1. A written proposal documenting the change, motivation, and migration plan.
2. Review and approval by at least one additional contributor.
3. A version bump according to the versioning policy below.
4. All dependent templates and artifacts MUST be updated before the amendment
   takes effect.

**Versioning policy**:
- MAJOR: Removal or redefinition of an existing principle.
- MINOR: Addition of a new principle or material expansion of existing guidance.
- PATCH: Wording clarifications, formatting fixes, or non-semantic refinements.

All PRs and code reviews MUST verify compliance with this constitution. Added
complexity MUST be justified; YAGNI principles apply throughout the project.

**Version**: 1.0.2 | **Ratified**: 2026-04-13 | **Last Amended**: 2026-04-25
