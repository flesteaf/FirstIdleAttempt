# Quickstart: Hacking Idle Game — Core Game Loop

**Date**: 2026-04-13
**Purpose**: End-to-end validation guide. Walk through this after each milestone
to confirm the core game loop is working as specified.

---

## Prerequisites

- Unity 6 (6000.4.2f1) installed with 2D URP template
- JetBrains Rider 2026.1.0.1 set as External Script Editor
- Project opens without errors in Unity Editor

---

## Step 1: Start a New Game

1. Open the project in Unity and enter Play Mode.
2. Confirm the terminal UI appears with a blinking cursor.
3. Confirm your starting balance shows `0.0000 BTC`.

**Expected**: Terminal is ready for input. No errors in the Console.

---

## Step 2: Scan for Networks

1. Type `scan` and press Enter.
2. Confirm 2–5 networks appear, each with an SSID and security level.
3. Type `scan` again and confirm you move to a new location with different networks.

**Expected**: Networks listed correctly, second scan changes location.

---

## Step 3: Inspect a Network

1. From the network list, pick a `[WEP]` network (e.g., `DIRECT-TV-2847`).
2. Type `scan network DIRECT-TV-2847`.
3. Confirm the security level (`WEP`) and a list of device IPs appear.

**Expected**: Security level matches; 1–4 devices shown.

---

## Step 4: Crack the Network

1. Type `crack WEP DIRECT-TV-2847`.
2. Confirm an error appears if you do not yet own the WEP crack tool.
3. Purchase the WEP crack tool from the store (type `store` or navigate the UI).
4. Retry `crack WEP DIRECT-TV-2847`.
5. Confirm "Access granted" message appears.

**Expected**: Correct tool gating; network marked as hacked after successful crack.

---

## Step 5: Scan a Device and Disable Its Firewall

1. From the device list, pick an IP (e.g., `192.168.1.42`).
2. Type `scan ip 192.168.1.42`.
3. Confirm firewall status (`ACTIVE` or `DISABLED`) and ports appear.
4. If firewall is `ACTIVE`, type `firewall disable`.
5. Confirm firewall is now `DISABLED`.

**Expected**: Firewall toggles correctly; error if tool not owned.

---

## Step 6: Inject a Miner

1. Type `inject miner`.
2. Confirm a success message appears showing income rate (e.g., `0.0012 BTC/s`).
3. Wait 30 seconds and check your balance — it should have increased.

**Expected**: Passive income starts within 30 seconds. Balance increments correctly.

---

## Step 7: Check Infected Devices

1. Type `show ips`.
2. Confirm `192.168.1.42` appears with `miner` and the correct income rate.

**Expected**: All infected devices listed accurately.

---

## Step 8: Offline Income

1. Note your current BTC balance.
2. Exit Play Mode (simulate closing the game).
3. Wait at least 10 seconds.
4. Re-enter Play Mode.
5. Confirm balance has increased by `elapsedSeconds × incomeRate` (approximately).

**Expected**: Offline income applied on load. Amount matches elapsed time × rate.

---

## Step 9: Purchase a Store Upgrade

1. Open the store.
2. Confirm at least 3 items are listed with prices and descriptions.
3. Purchase a PC component upgrade.
4. Confirm your BTC balance decreased by the correct amount.
5. Confirm the upgrade's effect is active (e.g., miner income rate increased).

**Expected**: Balance deducted, upgrade applied immediately.

---

## Step 10: Accept and Complete a Contract

1. Inject a bot into any accessible device (`inject bot`).
2. Open the contracts panel and confirm DDOS contracts are now listed.
3. Accept a contract and complete its objectives.
4. Confirm the contract is marked complete and reward is credited to your balance.

**Expected**: Contract gating works; reward applied correctly.

---

## Performance Check

After completing steps 1–10 with 10+ infected devices active:

1. Open Unity's Profiler window (Window > Analysis > Profiler).
2. Confirm no single frame exceeds 22.22 ms.
3. Confirm no per-frame GC allocations appear in the idle tick path.

**Expected**: Stable at 45+ FPS. No allocations in the tick system's hot path.

---

## Done

If all steps pass, the core game loop is validated. Proceed to `/speckit.tasks`
to generate the implementation task list.
