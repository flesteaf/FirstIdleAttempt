using System.Collections;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;
using HackYourWay.Data;

namespace HackYourWay.Tests.PlayMode
{
    /// <summary>
    /// Performance tests using <see cref="ProfilerRecorder"/> (Unity 6 built-in).
    /// Asserts that active-miner income ticks and file-listing commands stay below
    /// the 22.22 ms frame-time budget (Constitution Principle IV: 45 FPS floor).
    /// </summary>
    public class PerformanceTests
    {
        private const double MaxFrameTimeMs = 22.22;

        // ── T059: Income tick perf with 10+ miners ────────────────────────────

        [UnityTest]
        public IEnumerator IncomeService_TenMiners_60Ticks_AverageFrameTimeBelowBudget()
        {
            // --- Setup ---
            var config = ScriptableObject.CreateInstance<LocationConfigSO>();
            config.MinNetworks          = 1;
            config.MaxNetworks          = 1;
            config.MinDevicesPerNetwork = 12;
            config.MaxDevicesPerNetwork = 12;
            config.MinFilesPerDevice    = 0;
            config.MaxFilesPerDevice    = 0;
            config.SecurityDistribution = new float[] { 0f, 0f, 0f, 1f };
            config.MinRansomAmount      = 0.01f;
            config.MaxRansomAmount      = 0.10f;

            var svc    = new Services.LocationService(config);
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 0);

            var devices = svc.GetCurrentLocation().Networks[0].Devices;
            for (int i = 0; i < devices.Count; i++)
            {
                devices[i].ActiveMalware = new Malware
                {
                    Type       = MalwareType.Miner,
                    IncomeRate = 0.001,
                    Currency   = CurrencyType.Bitcoin,
                    InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks - 1
                };
            }

            var incomeService = new IncomeService(player, svc);

            // --- Measure: 60 ticks ---
            var recorder = new ProfilerRecorder(ProfilerCategory.Internal, "Main Thread", 64);
            recorder.Start();

            for (int tick = 0; tick < 60; tick++)
            {
                incomeService.OnTick(1.0);
                yield return null;
            }

            recorder.Stop();

            // --- Assert ---
            double avgMs = recorder.LastValue / 1_000_000.0; // nanoseconds → ms
            recorder.Dispose();

            Assert.Less(avgMs, MaxFrameTimeMs,
                $"Main-thread frame time {avgMs:F2} ms exceeds 22.22 ms budget with 10+ miners.");
        }

        // ── T064: LsCommand / CopyCommand perf with 100+ files ───────────────

        [UnityTest]
        public IEnumerator LsAndCopy_OneHundredFiles_NearZeroGcAllocation()
        {
            // --- Setup a device with 100+ files ---
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

            player.TargetedDevice  = device;
            player.TargetedNetwork = new Network { Ssid = "TestNet", SecurityLevel = SecurityLevel.None };

            var lsCmd   = new LsCommand(player);
            var copyCmd = new CopyCommand(player);

            // Warm up (avoid JIT allocation noise).
            lsCmd.Execute(new string[0]);
            copyCmd.Execute(new[] { "file_000.txt" });

            // --- Measure GC allocations ---
            var gcRecorder = new ProfilerRecorder(ProfilerCategory.Memory, "GC Allocated In Frame", 16);
            gcRecorder.Start();

            lsCmd.Execute(new string[0]);
            copyCmd.Execute(new[] { "file_050.txt" });

            yield return null;

            gcRecorder.Stop();
            long gcBytes = gcRecorder.LastValue;
            gcRecorder.Dispose();

            // Allow a small threshold for Unity's own overhead; target is ~0 for hot path.
            const long toleranceBytes = 2048;
            Assert.Less(gcBytes, toleranceBytes,
                $"LsCommand + CopyCommand allocated {gcBytes} bytes of GC; should be near zero.");
        }
    }
}
