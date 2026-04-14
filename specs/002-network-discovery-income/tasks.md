---
description: "Task list for Network Discovery Income — Direct Inject by IP/SSID"
---

# Tasks: Network Discovery Income — Direct Inject by IP/SSID

**Input**: Design documents from `specs/002-network-discovery-income/`
**Prerequisites**: plan.md ✅  spec.md ✅  contracts/command-schema.md ✅
**Depends on**: Feature 001-hacking-idle-core all phases complete

**Tests**: Included — Constitution Principle II mandates TDD (Red-Green-Refactor).
Tests MUST be written and confirmed FAILING before each implementation task.

---

## Format: `[ID] [P?] Description`

- **[P]**: Can run in parallel with other tasks at the same phase
- Paths are relative to repository root

---

## Phase 1: Enable Test Stub (Prerequisite — 1 word)

- [X] T002-000 Add `virtual` keyword to `GetCurrentLocation()` in
  `Assets/Scripts/Services/LocationService.cs`:

  ```csharp
  // Before:
  public Location GetCurrentLocation()

  // After:
  public virtual Location GetCurrentLocation()
  ```

  This allows the edit-mode `StubLocationService` (in Phase 2) to override the method
  without requiring a ScriptableObject. No other change to `LocationService`.

---

## Phase 2: Tests (write first — must FAIL before Phase 3)

- [X] T002-001 Append new test cases to `Assets/Tests/EditMode/InjectCommandTests.cs`.

  Add a second class `InjectCommandDirectInjectTests` **after** the closing `}` of the
  existing `InjectCommandTests` class, in the same file. Include a
  `StubLocationService` private inner class that extends `LocationService`, calls
  `base(null)`, and overrides `GetCurrentLocation()` to return a fixed `Location`.

  The constructor call `new InjectCommand(player, locationService)` MUST fail to
  compile until T002-002 is done — that compile failure is the required RED state.

  **9 test cases to write:**

  1. `DirectInject_ValidIpAndSsid_MinerInstalled`
     — hacked WPA2 network, firewall disabled → `result.Success == true`,
     `device.ActiveMalware.Type == MalwareType.Miner`.

  2. `DirectInject_NetworkNotFound_ReturnsError`
     — SSID not in location → `result.Success == false`,
     message contains `"not found"`.

  3. `DirectInject_NetworkNotAccessible_ReturnsError`
     — `IsHacked=false`, `SecurityLevel=WPA2` → `result.Success == false`,
     message contains `"not accessible"`.

  4. `DirectInject_OpenNetwork_BypassesCrackRequirement`
     — `SecurityLevel.None`, `IsHacked=false` → `result.Success == true`
     (open network waiver applies without cracking).

  5. `DirectInject_DeviceNotOnNetwork_ReturnsError`
     — valid SSID, wrong IP → `result.Success == false`,
     message contains `"not found on network"`.

  6. `DirectInject_FirewallActive_ReturnsError`
     — valid SSID + IP, `FirewallStatus.Active` → `result.Success == false`,
     message contains `"Firewall"`.

  7. `DirectInject_AlreadyInfected_ReturnsError`
     — device already has `ActiveMalware` set → `result.Success == false`,
     message contains `"already infected"`.

  8. `DirectInject_PartialArgs_OnlyIp_ReturnsUsageError`
     — `Execute(new[] { "miner", "192.168.1.20" })` (type + IP, no SSID) →
     `result.Success == false`, message contains `"Usage:"`.

  9. `DirectInject_LegacyNoArgs_StillWorks`
     — no IP/SSID args, `Player.TargetedDevice` set manually →
     `result.Success == true` (regression guard for v1.0.0 path).

  ⚠️ Confirm all 9 tests are RED before proceeding to Phase 3.

---

## Phase 3: Implementation

- [X] T002-002 Rewrite `Assets/Scripts/Services/Commands/InjectCommand.cs`:

  1. Add `private readonly LocationService _locationService;` field.
  2. Change constructor to `InjectCommand(Player player, LocationService locationService)`;
     add XML doc on the new `locationService` parameter.
  3. In `Execute()`, after the `args.Length < 1` guard, insert:
     - `if (args.Length == 2)` → return usage error (partial args — FR-008)
     - `if (args.Length >= 3)` → call `TryResolveDevice(args[1], args[2], ...)`;
       return error on failure, use resolved `(dev, net)` on success
     - `else` (args.Length == 1) → legacy path using `Player.TargetedDevice`
  4. Add private `TryResolveDevice(string ip, string ssid, out Device dev,
     out Network net, out string error)`:
     - `for` loop over `loc.Networks` to find by SSID (no LINQ — Principle IV)
     - Return error if SSID not found
     - Check `IsHacked || SecurityLevel == None`; return error if inaccessible
     - `for` loop over `network.Devices` to find by IP (no LINQ)
     - Return error if IP not found
     - Set `out` params on success; return `true`
  5. Add XML doc comment to `TryResolveDevice`.

  All error message strings MUST match `contracts/command-schema.md` v1.1.0 exactly.

  (verify T002-001 passes — all 9 new tests GREEN, all original tests still GREEN)

- [X] T002-003 Update `Assets/Scripts/Core/GameManager.cs` — `RegisterCommands()`:

  Change exactly one line:
  ```csharp
  // Before:
  CommandParser.Register("inject", new InjectCommand(p));

  // After:
  CommandParser.Register("inject", new InjectCommand(p, LocationService));
  ```

  `LocationService` is already initialised in `Bootstrap()` before
  `RegisterCommands()` is called — no additional field or initialisation required.

---

## Phase 4: Polish

- [X] T002-004 [P] Audit all new error strings in `InjectCommand.cs` against
  `specs/002-network-discovery-income/contracts/command-schema.md` v1.1.0.
  Verify exact string match for every error path.
  (Constitution Principle III)

- [X] T002-005 [P] Verify XML documentation is present and correct on all new
  constructor parameters and private methods added in T002-002.
  (Constitution Principle I)

---

## Dependencies & Execution Order

1. **T002-000** — make `GetCurrentLocation()` virtual (unblocks stub in T002-001)
2. **T002-001** — write tests; MUST be RED before T002-002
3. **T002-002** — InjectCommand rewrite; makes T002-001 GREEN
4. **T002-003** — GameManager 1-line fix; commit alongside T002-002
5. **T002-004, T002-005** — parallel polish after T002-002

## Parallel Opportunities

- T002-004 and T002-005 are independent and can run in parallel.
- T002-002 and T002-003 touch different files — can be committed in the same PR hunk.

---

## Checkpoint: Feature Complete When

- All 9 new `InjectCommandDirectInjectTests` pass.
- All original `InjectCommandTests` pass (no regression).
- `CoreLoopIntegrationTests` pass (no regression in core loop).
- Manual end-to-end (Play Mode):
  1. `scan` → see networks
  2. `scan network {SSID}` → see device IPs
  3. `inject miner {IP} {SSID}` on open network → success, no `scan ip` needed
  4. `inject miner {IP} {SSID}` on un-cracked secured network → error "not accessible"
  5. `show ips` → infected device listed
  6. Wait 5 seconds → BTC balance has increased

---

## Notes

- Total diff: ~80 lines added across 4 files, 1 word changed in LocationService.cs
- No new files. No new models. No new services. No new ScriptableObjects.
- [P] tasks touch different concerns — safe to parallelise
