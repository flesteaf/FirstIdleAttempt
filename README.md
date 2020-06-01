# FirstIdleAttempt
First attempt at an idle game in Unity 2D

## Overview
- [ ] [Instructions](#instructions)
- [ ] [Commands](#commands)
  - [x] [Scan](#scan)
  - [x] [Show](#show)
  - [ ] [Crack](#crack)
  - [ ] [Inject](#inject)
  - [ ] [Firewall](#firewall)
  - [ ] [Copy](#copy)
  - [ ] [ls](#ls)
  - [ ] [Ransomware](#ransomware)
- [ ] [Store](#store)
  - [ ] [PC Components](#pc-components)
  - [ ] [Software](#software)
- [ ] [Contracts](#contracts)
  - [ ] [DDOS Attacks](#ddos-attacks)
  - [ ] [Obtain file](#obtain-file)
  - [ ] [Facilitate attack](#facilitate-attack) (e.g. disable firewall for someone else, or crack security protection for someone else)
- [ ] [Bot scripting](#bot-scripting)

## Instructions
Attack order:
- scan
- crack SSID if needed
- scan network SSID
- scan ip IP - for firewall
- disable-firewall if needed
- inject miner - or anything else
- re-enable-firewall - if needed

## Commands
- [Scan](#scan)
- [Show](#show)
- [Crack](#crack)
- [Inject](#inject)
- [Firewall](#firewall)
- [Ransomware](#ransomware)

### Scan
The `scan` command can be used without parameters or with parameters

- `scan` - detects networks in the area, a second run of `scan` without parameters, will move you to a new location, with new networks to be discovered. Can be upgraded to show the security level details
- `scan network {SSID}` - scans for the security level of the given network `SSID` and also the available devices (IPs) in that network
- `scan ip {IP}` - determines the firewall status and if firewall is up, what ports are available
- `scan mac {MAC}` - determines the firewall status and if firewall is up, what ports are available

### Show
The `show` command present different data related to found networks and also details about the hacked one. Including their devices.
- `show networks` - presents all the currently found networks from the current location, including the already hacked ones
- `show ips` - presents all the infected ips and the type of malware that was injected

### Crack
`crack [Protection] {SSID}` command offers the possibility to infiltrate in a protected system. There are several `crack` tools, each for a specific protection level. The possible `crack` tools and their commands are: 
- WEP - `crack WEP {SSID}`  - used for hacking into systems with `WEP` protection level
- WPA - `crack WPA {SSID}`  - used for hacking into systems with `WPA` protection level
- WPA2 - `crack WPA2 {SSID}`  - used for hacking into systems with `WPA2` protection level

### Inject
This command is used to inject a piece of malware in order to mine for virtual coins (several are available upon upgrading the software), or convert that device into a tools for yourself
- `inject miner` - first available malware, used to mine bitcoins, in the beginning.
- `inject bot` - convert this device into a bot that can be used to trigger different attack or to automate attacks. Also unlock attack contracts
- `inject spammer` - with a spammer available, spam contracts are unlocked. Spam the world with e-mails or smss that brings in money
- `inject ransomware` - block the device. All the size of the device is encrypted by you and you can now request money to decrypt those files. For more details, check [Ransomware](#ransomware)
