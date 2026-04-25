using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using HackYourWay.Core;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Data;

namespace HackYourWay.Tests.PlayMode
{
    /// <summary>
    /// Play-mode integration tests for the US1 core loop:
    /// scan → crack → inject miner → passive income → offline income on reload.
    /// </summary>
    public class CoreLoopIntegrationTests
    {
        // ── Scene-less helpers ────────────────────────────────────────────────

        private static Services.LocationService BuildLocationService(LocationConfigSO config = null)
        {
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<LocationConfigSO>();
                config.MinNetworks        = 1;
                config.MaxNetworks        = 1;
                config.MinDevicesPerNetwork = 1;
                config.MaxDevicesPerNetwork = 1;
                config.MinFilesPerDevice  = 0;
                config.MaxFilesPerDevice  = 0;
                config.SecurityDistribution = new float[] { 0f, 0f, 0f, 1f }; // WPA2
                config.MinRansomAmount    = 0.01f;
                config.MaxRansomAmount    = 0.10f;
            }
            return new Services.LocationService(config);
        }

        // ── Test 1: Command pipeline drives income accumulation ────────────────

        [UnityTest]
        public IEnumerator ScanCrackInjectMiner_BalanceIncreasesAfterTwoTicks()
        {
            // --- Arrange ---
            var svc    = BuildLocationService();
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 0);
            player.UnlockedTools.Add(ToolType.CrackWPA2);
            player.UnlockedTools.Add(ToolType.FirewallDisable);

            var parser = new CommandParser();
            parser.Register("scan",     new HackYourWay.Services.Commands.ScanCommand(player, svc));
            parser.Register("crack",    new HackYourWay.Services.Commands.CrackCommand(player, svc));
            parser.Register("firewall", new HackYourWay.Services.Commands.FirewallCommand(player, svc));
            parser.Register("inject",   new HackYourWay.Services.Commands.InjectCommand(player, svc));

            var incomeService = new IncomeService(player, svc);

            // --- Act: simulate player commands ---
            svc.MoveToNextLocation(); // populate cache so scan has a current location
            parser.Parse("scan");

            var location = svc.GetCurrentLocation();
            var network  = location.Networks[0];
            var device   = network.Devices[0];

            // Crack the network.
            parser.Parse($"crack WPA2 {network.Ssid}");
            Assert.IsTrue(network.IsHacked, "Network should be hacked after crack.");

            // Disable firewall using explicit IP (no implicit targeting).
            network.IsHacked = true;
            parser.Parse($"firewall disable {device.Ip}");
            Assert.AreEqual(FirewallStatus.Disabled, device.FirewallStatus);

            // Inject miner using direct path: inject {type} {IP} {SSID}.
            parser.Parse($"inject miner {device.Ip} {network.Ssid}");
            Assert.IsNotNull(device.ActiveMalware, "Miner should be installed.");

            // Simulate two ticks via IncomeService.
            incomeService.OnTick(1.0);
            incomeService.OnTick(1.0);

            // --- Assert ---
            double balance = player.GetBalance(CurrencyType.Bitcoin);
            Assert.Greater(balance, 0.0, "Player should have earned BTC after two ticks.");

            yield return null;
        }

        // ── Test 2: Offline income applies on simulated reload ────────────────

        [UnityTest]
        public IEnumerator OfflineIncome_AppliesOnSimulatedReload()
        {
            // --- Arrange ---
            var svc    = BuildLocationService();
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 0);

            var location = svc.GetCurrentLocation();
            var device   = location.Networks[0].Devices[0];

            // Install a miner with a known rate.
            const double incomeRate  = 1.0; // 1 BTC/s
            long installedTicks = System.DateTime.UtcNow.Ticks - System.TimeSpan.TicksPerSecond * 5; // 5s ago
            device.ActiveMalware = new Malware
            {
                Type                = MalwareType.Miner,
                DeviceIp            = device.Ip,
                IncomeRate          = incomeRate,
                Currency            = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = installedTicks
            };

            // Mark the save as 3 seconds before install (so baseline = installedTicks).
            player.LastSaveUtcTicks =
                installedTicks - System.TimeSpan.TicksPerSecond * 2;

            // --- Act: simulate offline income calculation ---
            var malwareList = new System.Collections.Generic.List<Malware> { device.ActiveMalware };
            var gains = OfflineIncomeCalculator.Calculate(
                player.LastSaveUtcTicks,
                malwareList,
                System.DateTime.UtcNow.Ticks);

            for (int i = 0; i < gains.Count; i++)
                player.AddBalance(gains[i].Currency, gains[i].Amount);

            // --- Assert: ~5 seconds of income at 1 BTC/s (≥4 expected due to timing) ---
            double balance = player.GetBalance(CurrencyType.Bitcoin);
            Assert.GreaterOrEqual(balance, 4.0,
                $"Offline income should credit ~5 BTC (got {balance:F4}).");

            yield return null;
        }
    }
}
