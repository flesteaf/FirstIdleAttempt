# Command Schema Contract

**Feature**: Hacking Idle Game — Core Game Loop
**Date**: 2026-04-13
**Type**: Terminal UI — Player Command Interface

This document defines every player-facing terminal command: its syntax, valid
arguments, preconditions, success output, and error output. All command
implementations MUST conform exactly to this schema. Any change to a command
signature constitutes a breaking change and requires a schema version bump.

**Schema Version**: 1.0.0

---

## General Rules

- Commands are case-insensitive.
- Arguments are space-separated tokens.
- Unknown commands return: `Unknown command: '{input}'. Type 'help' for a list.`
- All output is plain text rendered in the terminal UI.
- Error messages MUST begin with `Error:` followed by a human-readable explanation.
- Success messages MUST be affirmative and specific (no bare "OK").

---

## scan

**Syntax**: `scan`
**Syntax**: `scan network {SSID}`
**Syntax**: `scan ip {IP}`
**Syntax**: `scan mac {MAC}`

### `scan` (no arguments)

**Precondition**: None

**Behaviour**:
- First call in a location: lists all discoverable networks at current location
- Second call in same location (or any subsequent call after networks are already
  listed): moves player to a new location and lists its networks

**Success output** (networks found):
```
Scanning area...
Found 3 network(s):

  [1] HomeNetwork_42      [WPA2]
  [2] DIRECT-TV-2847      [WEP]
  [3] OpenCafe            [OPEN]
```

**Success output** (moving to new location):
```
Moving to new area...
Scanning area...
Found 2 network(s):
  ...
```

**Error**: None defined (always produces output).

---

### `scan network {SSID}`

**Precondition**: Network with `{SSID}` must be in the discovered list at current location.

**Success output**:
```
Scanning HomeNetwork_42...
Security: WPA2
Devices found: 3

  192.168.1.1    (router)
  192.168.1.42
  192.168.1.55
```

**Error outputs**:
```
Error: Network 'HomeNetwork_42' not found. Run 'scan' first.
```

---

### `scan ip {IP}`

**Precondition**: Player must have access to the network that contains `{IP}`.

**Success output**:
```
Scanning 192.168.1.42...
Firewall: ACTIVE
Open ports: 22, 80, 443
```

```
Scanning 192.168.1.42...
Firewall: DISABLED
Open ports: 22, 80, 443
Malware: miner [Bitcoin] — 0.0042 BTC/s
```

**Error outputs**:
```
Error: IP '192.168.1.42' not found on any accessible network.
```

---

### `scan mac {MAC}`

**Precondition**: Same as `scan ip`. MAC address must belong to a device on an
accessible network.

**Success output**: Same format as `scan ip`.

**Error outputs**:
```
Error: MAC address 'AA:BB:CC:DD:EE:FF' not found on any accessible network.
```

---

## crack

**Syntax**: `crack {WEP|WPA|WPA2} {SSID}`

**Precondition**:
- Player must have discovered `{SSID}` via `scan`.
- Player must own the tool corresponding to `{TYPE}` (purchased from store).
- Network security level must match `{TYPE}`.

**Success output**:
```
Cracking HomeNetwork_42 [WPA2]...
Access granted. Network is now under your control.
```

**Error outputs**:
```
Error: You do not own a WPA2 cracking tool. Visit the store to purchase one.
Error: Network 'HomeNetwork_42' uses WEP security, not WPA2.
Error: Network 'HomeNetwork_42' not found. Run 'scan' first.
Error: Network 'HomeNetwork_42' is already hacked.
```

---

## inject

**Syntax**: `inject {miner|bot|spammer|ransomware}`

**Precondition**:
- Player must have a currently selected device (targeted via the last `scan ip`
  or `scan mac` result).
- Target device firewall must be `DISABLED`.
- Device must not already have an active malware payload.
- For `bot`, `spammer`, `ransomware`: corresponding software must be purchased.

**Success output** (miner):
```
Injecting miner into 192.168.1.42...
Miner installed. Generating 0.0012 BTC/s.
```

**Success output** (bot):
```
Injecting bot into 192.168.1.42...
Bot installed. Attack contracts are now available.
```

**Success output** (spammer):
```
Injecting spammer into 192.168.1.42...
Spammer installed. Spam contracts are now available.
Generating 0.0008 BTC/s.
```

**Success output** (ransomware):
```
Injecting ransomware into 192.168.1.42...
Device encrypted. Ransom demand: {ransom_amount} BTC.
Awaiting payment...
```
*`{ransom_amount}` is determined by the device's LocationConfigSO at generation time.*

**Error outputs**:
```
Error: No device targeted. Run 'scan ip {IP}' first.
Error: Firewall is active on 192.168.1.42. Disable it first.
Error: 192.168.1.42 is already infected with a miner.
Error: You do not own bot injection software. Visit the store to purchase it.
```

---

## firewall

**Syntax**: `firewall {disable|enable}`

**Precondition**:
- Player must have a currently targeted device.
- Player must own the `FirewallDisable` tool (purchased from store).

**Success output**:
```
Firewall disabled on 192.168.1.42.
```
```
Firewall re-enabled on 192.168.1.42.
```

**Error outputs**:
```
Error: No device targeted. Run 'scan ip {IP}' first.
Error: You do not own a firewall tool. Visit the store to purchase one.
Error: Firewall is already disabled on 192.168.1.42.
Error: Firewall is already active on 192.168.1.42.
```

---

## show

**Syntax**: `show networks`
**Syntax**: `show ips`

### `show networks`

**Precondition**: None.

**Success output**:
```
Known networks:

  [HACKED]  HomeNetwork_42      192.168.1.x (3 devices, 2 infected)
  [FOUND]   DIRECT-TV-2847      [WEP] — not cracked
  [FOUND]   OpenCafe            [OPEN] — not infiltrated
```

---

### `show ips`

**Precondition**: None.

**Success output**:
```
Infected devices:

  192.168.1.42    miner     0.0042 BTC/s
  192.168.1.55    bot       (no income)
  10.0.0.7        spammer   0.0008 BTC/s
```

**Output when none**:
```
No infected devices yet. Hack a network and inject malware to get started.
```

---

## ls

**Syntax**: `ls`

**Precondition**: Player must have a currently targeted device.

**Success output**:
```
Files on 192.168.1.42:
  documents/report_Q1.pdf    (2.4 MB)
  documents/passwords.txt    (1 KB)
  downloads/installer.exe    (45 MB)
```

---

## copy

**Syntax**: `copy {filename}`

**Precondition**:
- Player must have a currently targeted device.
- `{filename}` must exist on the targeted device (visible via `ls`).

**Success output**:
```
Copying documents/report_Q1.pdf...
File copied to your storage.
```

**Error outputs**:
```
Error: File 'report_Q1.pdf' not found on 192.168.1.42.
Error: No device targeted. Run 'scan ip {IP}' first.
```

---

## Schema Changelog

| Version | Date       | Change                           |
|---------|------------|----------------------------------|
| 1.0.0   | 2026-04-13 | Initial schema — core commands   |
