# Data Model: Hacking Idle Game — Core Game Loop

**Phase**: 1 — Design
**Date**: 2026-04-13
**Feature**: [spec.md](spec.md) | [research.md](research.md)

---

## Enumerations

### SecurityLevel
```
None    // Open network — no cracking required
WEP     // Requires WEP crack tool
WPA     // Requires WPA crack tool
WPA2    // Requires WPA2 crack tool
```

### MalwareType
```
Miner       // Passive income generation
Bot         // Enables automation and attack contracts
Spammer     // Unlocks spam contracts, additional passive income
Ransomware  // One-time payment demand on the device owner
```

### CurrencyType
```
Bitcoin     // Starting currency, always available
// Additional altcoins added here as unlocked via store milestones
// e.g., Monero, Ethereum — defined in CurrencyDefinitionSO assets
```

### ToolType
```
CrackWEP
CrackWPA
CrackWPA2
FirewallDisable
// Extended by store software purchases
```

### ContractType
```
DDOS            // Disrupt a target device
FileRetrieval   // Copy a file from a target device
FacilitatedAttack // Disable security for a third party
```

### FirewallStatus
```
Active
Disabled
```

### StoreItemCategory
```
PCComponent   // Improves hardware stats (mining speed, processing power)
Software      // Unlocks or enhances commands and tools
```

---

## Core Entities

### Location
Represents a virtual area the player can scan for networks.

| Field          | Type              | Notes                                      |
|----------------|-------------------|--------------------------------------------|
| id             | string            | Unique identifier                          |
| configId       | string            | Reference to LocationConfigSO              |
| networks       | List\<Network\>   | Populated on first visit via procedural gen|
| isVisited      | bool              | Whether the player has scanned here before |

**State transitions**:
- Unvisited → Visited: on first `scan` in this location
- Player moves to next location: on second `scan` without parameters

---

### Network
A discoverable Wi-Fi access point within a location.

| Field          | Type              | Notes                                      |
|----------------|-------------------|--------------------------------------------|
| ssid           | string            | Display name, unique within location       |
| securityLevel  | SecurityLevel     | Determines which crack tool is required    |
| devices        | List\<Device\>    | Populated after `scan network {SSID}`      |
| isHacked       | bool              | True after successful crack                |
| isScanned      | bool              | True after `scan network` is run           |

**Validation rules**:
- SSID must be non-empty and unique within its Location
- A network with `SecurityLevel.None` is accessible without cracking

**State transitions**:
- Undiscovered → Discovered: on `scan` at parent location
- Discovered → Scanned: on `scan network {SSID}`
- Scanned → Hacked: on successful `crack {TYPE} {SSID}` with correct tool

---

### Device
A machine connected to a hacked network.

| Field          | Type              | Notes                                      |
|----------------|-------------------|--------------------------------------------|
| ip             | string            | e.g., "192.168.1.42", unique in network    |
| mac            | string            | Hardware address                           |
| firewallStatus | FirewallStatus    | Must be Disabled before inject             |
| openPorts      | List\<int\>       | Available ports on this device             |
| activeMalware  | Malware?          | Null if not infected; one malware at a time|
| isScanned      | bool              | True after `scan ip` or `scan mac`         |
| files          | List\<DeviceFile\>| Procedurally generated files on this device|

**Validation rules**:
- `activeMalware` is null or a single Malware instance (one infection per device)
- Inject is only possible when `firewallStatus == Disabled` OR device's parent network has `SecurityLevel.None`
- Maximum 50 devices may be simultaneously infected across all locations

### DeviceFile (nested within Device)

| Field     | Type   | Notes                              |
|-----------|--------|------------------------------------|
| name      | string | Display filename (e.g., "report.pdf") |
| path      | string | Full path (e.g., "documents/report.pdf") |
| sizeBytes | long   | File size for display purposes     |

**State transitions**:
- Unscanned → Scanned: on `scan ip {IP}` or `scan mac {MAC}`
- Firewall Active → Disabled: on `firewall disable`
- Firewall Disabled → Active: on `firewall enable`
- Clean → Infected: on `inject {type}`

---

### Malware
A payload installed on a Device.

| Field          | Type          | Notes                                         |
|----------------|---------------|-----------------------------------------------|
| type           | MalwareType   | Determines behaviour and income/contract access|
| deviceIp       | string        | IP of the host Device                         |
| incomeRate     | double        | Currency units per second (for Miner/Spammer) |
| currency       | CurrencyType  | Which currency this malware generates          |
| installedAt    | long          | UTC ticks at time of injection (for offline)  |

**Validation rules**:
- Only Miner and Spammer have non-zero `incomeRate`
- `currency` must be in the player's unlocked currency set at injection time

---

### Player
Singleton; persists across sessions via save system.

| Field              | Type                            | Notes                               |
|--------------------|---------------------------------|-------------------------------------|
| balances           | Dictionary\<CurrencyType,double\>| One entry per unlocked currency     |
| unlockedTools      | HashSet\<ToolType\>             | Grows via store purchases           |
| unlockedCurrencies | HashSet\<CurrencyType\>         | Bitcoin unlocked from start         |
| purchasedItems     | List\<string\>                  | Store item IDs                      |
| activeContracts    | List\<Contract\>                | Currently accepted contracts        |
| completedContracts | List\<string\>                  | Contract IDs                        |
| copiedFiles        | List\<string\>                  | File paths copied via CopyCommand   |
| lastSaveUtcTicks   | long                            | For offline income calculation      |

---

### StoreItem
Defined as a ScriptableObject asset; purchased instances referenced by ID.

| Field          | Type               | Notes                                    |
|----------------|--------------------|------------------------------------------|
| id             | string             | Unique identifier                        |
| displayName    | string             | Shown in store listing                   |
| description    | string             | Effect description for the player        |
| category       | StoreItemCategory  | PCComponent or Software                  |
| price          | double             | Cost in bitcoin (or specified currency)  |
| priceCurrency  | CurrencyType       | Currency used to purchase                |
| unlocksToolType| ToolType?          | If Software: tool this item unlocks      |
| unlocksCurrency| CurrencyType?      | If milestone: altcoin this item unlocks  |
| incomeMultiplier| double            | If PCComponent: multiplies all income    |

---

### Contract
An optional objective the player can accept for bonus rewards.

| Field              | Type            | Notes                                      |
|--------------------|-----------------|--------------------------------------------|
| id                 | string          | Unique identifier                          |
| type               | ContractType    | DDOS / FileRetrieval / FacilitatedAttack   |
| description        | string          | Player-facing objective text               |
| requiredMalware    | MalwareType?    | e.g., Bot required for DDOS contracts      |
| objectives         | List\<Objective\>| Ordered list of steps to complete         |
| reward             | double          | Currency amount on completion              |
| rewardCurrency     | CurrencyType    | Which currency the reward is paid in       |
| isCompleted        | bool            |                                            |

### Objective (nested within Contract)

| Field      | Type    | Notes                                             |
|------------|---------|---------------------------------------------------|
| description| string  | e.g., "Disable firewall on 192.168.1.42"          |
| isComplete | bool    |                                                   |

---

## Save Data Aggregate

All fields below are serialized to `save.json` on session end.

```
SaveData
├── player: PlayerSaveData
│   ├── balances: List<CurrencyBalance>   (CurrencyType + double, for JsonUtility compat)
│   ├── unlockedTools: List<ToolType>
│   ├── unlockedCurrencies: List<CurrencyType>
│   ├── purchasedItems: List<string>
│   ├── activeContracts: List<ContractSaveData>
│   ├── completedContracts: List<string>
│   ├── copiedFiles: List<string>
│   └── lastSaveUtcTicks: long
├── locations: List<LocationSaveData>
│   └── (id, networks: List<NetworkSaveData>
│           └── devices: List<DeviceSaveData>)
└── version: int   (save schema version for future migration)
```

---

## Relationships

```
Location  1 ──── * Network
Network   1 ──── * Device
Device    1 ──── 0..1 Malware
Player    1 ──── * StoreItem (purchased references)
Player    1 ──── * Contract (active + completed)
Malware       ──── 1 CurrencyType
Contract      ──── 1..* Objective
```
