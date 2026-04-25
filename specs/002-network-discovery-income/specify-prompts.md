# Specify Prompts: Network Discovery Income

**Feature**: 002-network-discovery-income
**Date**: 2026-04-13

This file records the speckit prompt text for each phase of this feature's
documentation. Pass each section's prompt to the corresponding speckit command
to regenerate or update the associated output file.

---

## /speckit.specify

**Output**: `specs/002-network-discovery-income/spec.md`

**Prompt**:

> Feature name: network-discovery-income
>
> Context: This is a terminal-style hacking idle game built in Unity 6 (C# 9).
> Players use a terminal UI to: scan for networks → crack security → scan devices →
> inject malware → earn passive income. The existing inject command syntax is
> `inject {miner|bot|spammer|ransomware}` and requires a prior `scan ip {IP}` to
> set Player.TargetedDevice before injection can proceed.
>
> Request: "ensure that the basic income generation is possible through network
> discovery, using 'scan' command, and infecting the discovered devices in those
> networks, using 'inject miner' and specifying the ip and network of the device
> to infect."
>
> This means adding a new optional variant for all malware types:
>   inject {miner|bot|spammer|ransomware} {IP} {SSID}
>
> Where IP and SSID are OPTIONAL arguments. If omitted: falls back to
> Player.TargetedDevice (existing behaviour, full backward compatibility).
> If both provided: resolves the device from the current location's network data.
>
> The new {IP} {SSID} form must:
> 1. Validate the SSID exists at the current location
> 2. Validate the network is accessible (IsHacked=true OR SecurityLevel.None)
> 3. Find the device by IP in that network
> 4. Apply all existing checks (firewall, already-infected, software ownership)
> 5. Install the malware
>
> Partial args (IP without SSID) must return a usage error.

---

## /speckit.plan

**Output**: `specs/002-network-discovery-income/plan.md`

**Prompt**:

> Input: specs/002-network-discovery-income/spec.md
>
> Technical context for the existing codebase:
> - InjectCommand constructor: InjectCommand(Player player) — no LocationService
> - GameManager registers it: CommandParser.Register("inject", new InjectCommand(p))
> - LocationService.GetCurrentLocation() returns Location with Networks + Devices populated
> - ScanCommand, CrackCommand, ShowCommand already take LocationService as a ctor param
> - All commands implement ICommand: Execute(string[] args) → CommandResult
> - No IService interfaces exist — services are constructed directly
> - Constitution rules: SRP, no LINQ in hot paths, XML docs on all public APIs, TDD-first
> - Edit-mode tests use plain C# (no MonoBehaviour). ScriptableObjects cannot be
>   instantiated in edit-mode without a stub/override.
>
> Design the implementation plan. Scope: no new files. Changes confined to
> InjectCommand.cs, LocationService.cs (1 word), GameManager.cs (1 line),
> InjectCommandTests.cs (append tests + stub).
>
> Include architecture decisions for:
> - Why LocationService is added to InjectCommand ctor (not resolved in GameManager)
> - Why Player.TargetedDevice is NOT updated in the direct-inject path
> - Why current location only (not searching all cached locations)
> - Why `virtual` on GetCurrentLocation rather than ILocationService interface

---

## /speckit.contract

**Output**: `specs/002-network-discovery-income/contracts/command-schema.md`

**Prompt**:

> Input: specs/002-network-discovery-income/spec.md + plan.md
>
> Base schema: specs/001-hacking-idle-core/contracts/command-schema.md v1.0.0
>
> Amend only the inject command. This is a non-breaking amendment (optional args only).
> Bump schema version to 1.1.0.
>
> Define:
> - The new optional {IP} {SSID} syntax alongside the existing syntax
> - Argument rules: both required if either provided, usage error for partial args
> - All new error messages with EXACT text (will be string-matched in unit tests):
>     "Network '{SSID}' not found at current location. Run 'scan' first."
>     "Network '{SSID}' is not accessible. Crack it first or target an open network."
>     "Device '{IP}' not found on network '{SSID}'."
>     "Usage: inject {miner|bot|spammer|ransomware} [{IP} {SSID}]"
> - Success output format (same as v1.0.0, no change)
> - Schema changelog entry for v1.1.0

---

## /speckit.tasks

**Output**: `specs/002-network-discovery-income/tasks.md`

**Prompt**:

> Input: specs/002-network-discovery-income/spec.md + plan.md + contracts/command-schema.md
>
> Generate TDD-first tasks. Constitution Principle II: tests MUST be written and
> confirmed FAILING before any implementation task begins.
>
> Scope constraints:
> - No new files — all changes are modifications to existing files
> - 4 files total: InjectCommand.cs, LocationService.cs, GameManager.cs, InjectCommandTests.cs
> - No new models, services, ScriptableObjects, or UI
>
> Task breakdown:
> - Phase 1: Make GetCurrentLocation() virtual (prerequisite for test stub, 1 word)
> - Phase 2: Write 9 test cases — MUST fail before Phase 3
> - Phase 3: Implement InjectCommand changes + GameManager 1-line update
> - Phase 4: Schema audit + XML doc verification (parallel)
>
> 9 test cases (include exact names and what each exercises):
> 1. DirectInject_ValidIpAndSsid_MinerInstalled
> 2. DirectInject_NetworkNotFound_ReturnsError
> 3. DirectInject_NetworkNotAccessible_ReturnsError
> 4. DirectInject_OpenNetwork_BypassesCrackRequirement
> 5. DirectInject_DeviceNotOnNetwork_ReturnsError
> 6. DirectInject_FirewallActive_ReturnsError
> 7. DirectInject_AlreadyInfected_ReturnsError
> 8. DirectInject_PartialArgs_OnlyIp_ReturnsUsageError
> 9. DirectInject_LegacyNoArgs_StillWorks (regression guard)
>
> The test stub: inner class StubLocationService extends LocationService, calls
> base(null), overrides GetCurrentLocation() to return a fixed Location.
> Requires LocationService to accept null LocationConfigSO in its constructor.

---

## /speckit.implement

**Output**: Modified source files (see plan.md for exact diffs)

**Prompt**:

> Input: specs/002-network-discovery-income/tasks.md
>
> Implement tasks T002-000 through T002-003 in order:
>
> T002-000: In Assets/Scripts/Services/LocationService.cs, add the `virtual` keyword
> to GetCurrentLocation(). One word change only.
>
> T002-001: Append class InjectCommandDirectInjectTests to
> Assets/Tests/EditMode/InjectCommandTests.cs. Include the StubLocationService inner
> class that extends LocationService with base(null) and overrides GetCurrentLocation().
> Write all 9 test cases. Confirm they fail to compile (RED) before T002-002.
>
> T002-002: Rewrite Assets/Scripts/Services/Commands/InjectCommand.cs to:
> - Add LocationService constructor param
> - Handle args.Length == 2 as partial-args usage error
> - Handle args.Length >= 3 as direct-resolve path via TryResolveDevice()
> - Add private TryResolveDevice() with for-loops (no LINQ)
> - All error strings must match contracts/command-schema.md v1.1.0 exactly
>
> T002-003: In Assets/Scripts/Core/GameManager.cs RegisterCommands(), change:
>   new InjectCommand(p)  →  new InjectCommand(p, LocationService)
>
> After all tasks: run Unity Test Runner, confirm all original + new tests pass.
