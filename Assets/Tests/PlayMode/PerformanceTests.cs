using System.Diagnostics;
using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;
using HackYourWay.Data;

namespace HackYourWay.Tests.PlayMode
{
    /// <summary>
    /// Performance tests using <see cref="Stopwatch"/>.
    /// Asserts that active-miner income ticks and file-listing commands stay below
    /// the 22.22 ms frame-time budget (Constitution Principle IV: 45 FPS floor).
    /// Runs within GDUnit4 (Godot runtime required for LocationConfigSO instantiation).
    /// </summary>
    public class PerformanceTests
    {
        private const double MaxFrameTimeMs = 22.22;

        // ── T059: Income tick perf with 10+ miners ────────────────────────────

        [Test]
        public void IncomeService_TenMiners_60Ticks_AverageTimeBelowBudget()
        {
            // --- Setup ---
            var config = new LocationConfigSO
            {
                MinNetworks          = 1,
                MaxNetworks          = 1,
                MinDevicesPerNetwork = 12,
                MaxDevicesPerNetwork = 12,
                MinFilesPerDevice    = 0,
                MaxFilesPerDevice    = 0,
                SecurityDistribution = new float[] { 0f, 0f, 0f, 1f },
                MinRansomAmount      = 0.01f,
                MaxRansomAmount      = 0.10f,
            };

            var svc    = new Services.LocationService(config);
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 0);

            var devices = svc.GetCurrentLocation().Networks[0].Devices;
            for (int i = 0; i < devices.Count; i++)
            {
                devices[i].ActiveMalware = new Malware
                {
                    Type                = MalwareType.Miner,
                    IncomeRate          = 0.001,
                    Currency            = CurrencyType.Bitcoin,
                    InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks - 1
                };
            }

            var incomeService = new IncomeService(player, svc);

            // --- Measure: 60 ticks ---
            var sw = Stopwatch.StartNew();

            for (int tick = 0; tick < 60; tick++)
                incomeService.OnTick(1.0);

            sw.Stop();

            double avgMs = sw.Elapsed.TotalMilliseconds / 60.0;

            // --- Assert ---
            Assert.Less(avgMs, MaxFrameTimeMs,
                $"Average tick time {avgMs:F2} ms exceeds 22.22 ms budget with 10+ miners.");
        }

        // ── T064: LsCommand / CopyCommand perf with 100+ files ───────────────

        [Test]
        public void LsAndCopy_OneHundredFiles_CompletesWithinBudget()
        {
            var player = new Player();
            var device = new Device { Ip = "192.168.99.1", FirewallStatus = FirewallStatus.Disabled };
            for (int f = 0; f < 120; f++)
            {
                device.Files.Add(new DeviceFile
                {
                    Name      = $"file_{f:D3}.txt",
                    Path      = $"documents/file_{f:D3}.txt",
                    SizeBytes = 1024 * (f + 1)
                });
            }

            var network  = new Network { Ssid = "TestNet", SecurityLevel = SecurityLevel.None };
            network.Devices.Add(device);
            var location = new Location { Id = "1", Name = "node_77" };
            location.Networks.Add(network);

            var svc     = new PerfStubLocationService(location);
            var lsCmd   = new LsCommand(svc);
            var copyCmd = new CopyCommand(player, svc);

            // Warm up.
            lsCmd.Execute(new[] { device.Ip });
            copyCmd.Execute(new[] { "file_000.txt", device.Ip });

            // --- Measure ---
            var sw = Stopwatch.StartNew();

            lsCmd.Execute(new[] { device.Ip });
            copyCmd.Execute(new[] { "file_050.txt", device.Ip });

            sw.Stop();

            Assert.Less(sw.Elapsed.TotalMilliseconds, MaxFrameTimeMs,
                $"LsCommand + CopyCommand took {sw.Elapsed.TotalMilliseconds:F2} ms; must stay under 22.22 ms.");
        }

        // ── Stub ──────────────────────────────────────────────────────────────

        private class PerfStubLocationService : Services.LocationService
        {
            private readonly Models.Location _location;

            public PerfStubLocationService(Models.Location location) : base(null)
            {
                _location = location;
            }

            public override Models.Location GetCurrentLocation() => _location;
        }
    }
}
