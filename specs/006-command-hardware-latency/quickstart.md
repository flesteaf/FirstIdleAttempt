# Quickstart: Adding Hardware-Aware Latency to a Command

## Overview

All terminal commands implement `ICommand`. Instant commands (show, help, forget, move) need no changes — the default `GetLatency` returns `0f`. For commands with a progress bar, override `GetLatency` and delegate to `CommandLatencyService`.

---

## 1. Pick the right formula category

| Your command does… | Use |
|---|---|
| Local password cracking | `ProfileCrack` (CPU × GPU) |
| Push/pull data over network to a device | Bottleneck: `min(playerInternetTier, targetBandwidthTier)` |
| Scan the area (no specific target device) | `ProfileAreaScan` (player Internet tier only) |
| Nothing time-consuming | No override — default `0f` is correct |

---

## 2. Accept `CommandLatencyService` in your constructor

```csharp
public class MyCommand : ICommand
{
    private readonly Player                 _player;
    private readonly LocationService        _locationService;
    private readonly CommandLatencyService  _latencyService;

    public MyCommand(Player player, LocationService locationService, CommandLatencyService latencyService)
    {
        _player          = player;
        _locationService = locationService;
        _latencyService  = latencyService;
    }
```

Register in `GameManager.RegisterCommands()`:
```csharp
CommandParser.Register("myverb", new MyCommand(Player, LocationService, CommandLatencyService));
```

---

## 3. Override `GetLatency`

### Network-transfer command (bottleneck model)

```csharp
public float GetLatency(string[] args)
{
    // args[0] = target IP (example; adapt to your arg layout)
    Device target = _locationService?.FindDevice(args.Length > 0 ? args[0] : null);
    return _latencyService.CalculateLatency(new CommandLatencyContext
    {
        CommandVerb   = "myverb",
        TargetDevice  = target,       // null → service uses tier-5 default (no bottleneck)
        FileSizeBytes = 0
    });
}
```

### Compute-bound command (crack pattern)

```csharp
public float GetLatency(string[] args)
{
    var level = ParseSecurityLevel(args); // your parse helper
    return _latencyService.CalculateLatency(new CommandLatencyContext
    {
        CommandVerb   = "crack",
        SecurityLevel = level,
        TargetDevice  = null,
        FileSizeBytes = 0
    });
}
```

### Instant command

No override needed. The default interface method returns `0f`.

---

## 4. Write the test first (Red-Green-Refactor)

```csharp
[Test]
public void MyCommand_Tier1Player_Tier1Target_ReturnsBaseLatency()
{
    var player   = new Player { CpuTier = 1, InternetTier = 1, GpuTier = 0 };
    var service  = new CommandLatencyService(player);
    var context  = new CommandLatencyContext { CommandVerb = "myverb", TargetDevice = new Device { BandwidthTier = 1 } };
    float result = service.CalculateLatency(context);
    // At tier 1 / tier 1: speedup = 1.0, so result == base time for that verb
    Assert.That(result, Is.EqualTo(ExpectedBaseSec).Within(0.001f));
}

[Test]
public void MyCommand_MaxTiers_IsAtLeastFourTimesBaseForNetworkCommand()
{
    var player  = new Player { CpuTier = 5, InternetTier = 5, GpuTier = 3 };
    var service = new CommandLatencyService(player);
    var ctxBase = new CommandLatencyContext { CommandVerb = "myverb", TargetDevice = new Device { BandwidthTier = 1 } };
    var ctxMax  = new CommandLatencyContext { CommandVerb = "myverb", TargetDevice = new Device { BandwidthTier = 5 } };
    // Bottleneck at tier1 target caps speedup; full 4x only visible against tier5 target
    float baseTime = service.CalculateLatency(ctxBase);
    // baseTime should match tier-1-vs-tier-1 result (bottleneck = tier 1)
    Assert.That(baseTime, Is.EqualTo(TierOneTierOneResult).Within(0.001f));
}
```

---

## 5. Checklist before opening a PR

- [ ] `GetLatency` returns `0f` for instant paths (e.g., early-exit error cases)
- [ ] `GetLatency` does not modify any state (pure read-only)
- [ ] Constructor receives `CommandLatencyService`; `GameManager.RegisterCommands()` wires it
- [ ] Unit test for base-tier and max-tier cases in `CommandLatencyServiceTests.cs`
- [ ] `scan ip` output shows `CPU Tier` / `BW Tier` if your command reveals that info (FR-016)
- [ ] No new magic number literals — use named constants or service profile constants
