# Research: Hacking Idle Game — Core Game Loop

**Phase**: 0 — Research
**Date**: 2026-04-13
**Feature**: [spec.md](spec.md)

---

## 1. Idle Game Architecture in Unity

**Decision**: Timer-based tick system using a dedicated `TickManager` MonoBehaviour with a fixed interval (e.g., 1 second), not per-frame `Update()`.

**Rationale**: Per-frame accumulation is framerate-dependent and creates GC pressure from floating-point drift over long sessions. A coroutine-based `WaitForSeconds` or an `InvokeRepeating` pattern fires at a predictable interval, allows easy pause/resume, and generates zero per-frame allocations. This directly satisfies Constitution Principle IV (performance — no allocations in hot paths).

**Alternatives considered**:
- Per-frame `Update()` accumulation — rejected: allocation pressure, framerate coupling, violates Principle IV
- Unity's `Scheduler` / `IJobSystem` — rejected: overkill for single-player idle game at this scale
- Event-driven reactive pattern (UniRx) — rejected: adds external dependency; unnecessary for MVP

---

## 2. Offline Income Calculation

**Decision**: Persist a UTC timestamp (`DateTime.UtcNow.Ticks`) in the save file on application quit/pause, and a per-malware `installedAtUtcTicks` at injection time. On load, for each active malware compute `baselineTicks = max(malware.installedAtUtcTicks, player.lastSaveUtcTicks)` and `elapsedSeconds = (DateTime.UtcNow.Ticks - baselineTicks) / TimeSpan.TicksPerSecond`, then apply `income = elapsedSeconds × malware.incomeRate`.

**Rationale**: Full offline accrual (no cap) was specified in Q1. Using `max(installedAt, lastSave)` prevents over-counting income for malware installed mid-session: a miner installed 10 seconds before save should not be credited a full offline period starting from last session. UTC avoids timezone/daylight-saving drift. No floating-point accumulation is needed — elapsed time is a one-shot multiplication on load.

**Alternatives considered**:
- Capped offline progress (e.g., 8 hours) — rejected: player chose uncapped (option C)
- Cloud time API validation — rejected: game is offline-only, no real-money at stake

---

## 3. Progressive Multi-Currency Architecture

**Decision**: `ScriptableObject`-based currency definitions (`CurrencyDefinitionSO`) keyed by a `CurrencyType` enum. Player balance is a `Dictionary<CurrencyType, double>`. Altcoin currencies are unlocked by reaching store upgrade milestones, stored in a `HashSet<CurrencyType>` on the Player save data.

**Rationale**: ScriptableObjects allow designers to add currencies without code changes. `double` (not `float`) prevents precision loss at large idle-game numbers. Enum-keyed dictionary serializes cleanly to JSON via a wrapper.

**Alternatives considered**:
- `float` balance — rejected: precision loss above ~16M; idle games routinely exceed this
- Addressable currency assets — rejected: overkill; ScriptableObjects suffice for single-player
- Single currency forever — rejected: player chose progressive multi-currency (option B)

---

## 4. Terminal Command Parser

**Decision**: Single-responsibility `CommandParser` class using a `Dictionary<string, ICommand>` registry. Input string is tokenized by whitespace; the first token is the command verb, remaining tokens are arguments passed to the handler. Each command is a separate class implementing `ICommand`.

**Rationale**: Command pattern isolates each verb, making individual commands independently unit-testable (Constitution Principle II). New commands are registered without modifying the parser. This mirrors how real CLIs work and matches the game's hacker theme.

**Alternatives considered**:
- Switch statement in one class — rejected: violates SRP (Principle I), hard to test in isolation
- Regex-based parser — rejected: overkill for a small fixed command vocabulary
- Visual Scripting nodes — rejected: not version-controllable, violates Principle I

---

## 5. Save System

**Decision**: JSON serialization via `JsonUtility` (Unity built-in) to `Application.persistentDataPath/save.json`. A `SaveData` plain C# object aggregates all serializable game state. Save is triggered on application quit, application pause (mobile-ready), and after any significant state change (purchase, contract completion).

**Rationale**: `JsonUtility` is zero-dependency, allocates no unmanaged memory, and is fast enough for a single save file. Unity's `Application.persistentDataPath` is the correct cross-platform path.

**Alternatives considered**:
- Newtonsoft Json.NET — considered: better Dictionary support; deferred to post-MVP if `JsonUtility` Dictionary workarounds prove cumbersome
- PlayerPrefs — rejected: not suitable for complex structured data; not portable
- Binary serialization — rejected: not human-readable, harder to debug

---

## 6. Network/Device Procedural Generation

**Decision**: Networks and devices are procedurally generated from seed-based configuration `ScriptableObject`s (`LocationConfigSO`) defining distributions of security levels, device counts, and port combinations. Each location generates a fixed set of networks on first visit. Location IDs are sequential integers converted to strings (`"1"`, `"2"`, …), incremented each time the player moves; the integer value doubles as the PRNG seed, ensuring deterministic and reproducible generation for the same location.

**Rationale**: Allows authored content (location configs) without hand-crafting every network. Seed-based generation is deterministic and reproducible for the same save data.

**Alternatives considered**:
- Hand-authored level files — rejected: doesn't scale, high content authoring cost
- Fully random (no seed) — rejected: not reproducible; a player reloading would see different networks
