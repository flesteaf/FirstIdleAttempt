# Command Schema Contract — Network Discovery Income

**Feature**: Network Discovery Income — Direct Inject by IP/SSID
**Date**: 2026-04-13
**Type**: Terminal UI — Player Command Interface (Amendment)
**Base Schema**: `specs/001-hacking-idle-core/contracts/command-schema.md` v1.0.0

**Schema Version**: 1.1.0
**Breaking change**: No — new arguments are optional; all v1.0.0 behaviour is preserved.

---

## Scope

This document amends only the `inject` command. All other commands remain as defined
in the base schema v1.0.0. Implementations MUST satisfy both documents.

---

## inject (amended)

**Syntax v1.0.0** (unchanged, still valid):
```
inject {miner|bot|spammer|ransomware}
```

**Syntax v1.1.0** (new optional direct-target form):
```
inject {miner|bot|spammer|ransomware} {IP} {SSID}
```

**Argument rules**:
- `{IP}` and `{SSID}` are BOTH optional.
- If neither is provided: existing targeting behaviour (uses `Player.TargetedDevice`).
- If BOTH are provided: direct-resolve behaviour (looks up device from current location).
- If exactly ONE trailing argument is provided: usage error (see error outputs below).

### Preconditions — direct-resolve form (IP + SSID both present)

1. `{SSID}` must identify a network in the player's **current** location.
2. That network must be accessible: `IsHacked == true` OR `SecurityLevel == None`.
3. `{IP}` must identify a device in that network.
4. Device firewall must be `DISABLED` (open-network waiver: `SecurityLevel.None` bypasses this check).
5. Device must not already have an active malware payload.
6. For `bot`, `spammer`, `ransomware`: corresponding software must be purchased.

### Success outputs — direct-resolve form

Success output is identical to v1.0.0. The IP shown is the resolved device's IP.

**Miner**:
```
Injecting miner into 192.168.1.42...
Miner installed. Generating 0.0012 BTC/s.
```

**Bot**:
```
Injecting bot into 192.168.1.42...
Bot installed. Attack contracts are now available.
```

**Spammer**:
```
Injecting spammer into 192.168.1.42...
Spammer installed. Spam contracts are now available.
Generating 0.0008 BTC/s.
```

**Ransomware**:
```
Injecting ransomware into 192.168.1.42...
Device encrypted. Ransom demand: 0.0500 BTC.
Awaiting payment...
```

### Error outputs — direct-resolve form (new in v1.1.0)

```
Error: Network '{SSID}' not found at current location. Run 'scan' first.
Error: Network '{SSID}' is not accessible. Crack it first or target an open network.
Error: Device '{IP}' not found on network '{SSID}'.
Error: Usage: inject {miner|bot|spammer|ransomware} [{IP} {SSID}]
```

Existing v1.0.0 errors remain unchanged and apply to both forms:
```
Error: No device targeted. Run 'scan ip {IP}' first.
Error: Firewall is active on {IP}. Disable it first.
Error: {IP} is already infected with a {type}.
Error: You do not own bot injection software. Visit the store to purchase it.
Error: You do not own spammer software. Visit the store to purchase it.
Error: You do not own ransomware software. Visit the store to purchase it.
```

---

## Schema Changelog

| Version | Date       | Change                                                        |
|---------|------------|---------------------------------------------------------------|
| 1.0.0   | 2026-04-13 | Initial schema — core commands                                |
| 1.1.0   | 2026-04-13 | inject: optional {IP} {SSID} args for direct device targeting |
