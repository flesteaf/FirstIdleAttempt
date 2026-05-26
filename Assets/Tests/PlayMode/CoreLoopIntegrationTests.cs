using NUnit.Framework;
using HackYourWay.Core;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Data;

namespace HackYourWay.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for the US1 core loop:
    /// scan → crack → inject miner → passive income → offline income on reload.
    /// Runs within GDUnit4 (Godot runtime required for LocationConfigSO instantiation).
    /// </summary>
    public class CoreLoopIntegrationTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static Services.LocationService BuildLocationService(LocationConfigSO config = null)
        {
            if (config == null)
            {
                config = new LocationConfigSO
                {
                    MinNetworks          = 1,
                    MaxNetworks          = 1,
                    MinDevicesPerNetwork = 1,
                    MaxDevicesPerNetwork = 1,
                    MinFilesPerDevice    = 0,
                    MaxFilesPerDevice    = 0,
                    SecurityDistribution = new float[] { 0f, 0f, 0f, 1f },
                    MinRansomAmount      = 0.01f,
                    MaxRansomAmount      = 0.10f,
                };
            }
            return new Services.LocationService(config);
        }

        // ── Test 1: Command pipeline drives income accumulation ────────────────

        [Test]
        public void ScanCrackInjectMiner_BalanceIncreasesAfterTwoTicks()
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

            // --- Act ---
            svc.MoveToNextLocation();
            parser.Parse("scan");

            var location = svc.GetCurrentLocation();
            var network  = location.Networks[0];
            var device   = network.Devices[0];

            parser.Parse($"crack WPA2 {network.Ssid}");
            Assert.IsTrue(network.IsHacked, "Network should be hacked after crack.");

            network.IsHacked = true;
            parser.Parse($"firewall disable {device.Ip}");
            Assert.AreEqual(FirewallStatus.Disabled, device.FirewallStatus);

            parser.Parse($"inject miner {device.Ip} {network.Ssid}");
            Assert.IsNotNull(device.ActiveMalware, "Miner should be installed.");

            incomeService.OnTick(1.0);
            incomeService.OnTick(1.0);

            // --- Assert ---
            double balance = player.GetBalance(CurrencyType.Bitcoin);
            Assert.Greater(balance, 0.0, "Player should have earned BTC after two ticks.");
        }

        // ── Test 2: Offline income applies on simulated reload ────────────────

        [Test]
        public void OfflineIncome_AppliesOnSimulatedReload()
        {
            // --- Arrange ---
            var svc    = BuildLocationService();
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 0);

            var location = svc.GetCurrentLocation();
            var device   = location.Networks[0].Devices[0];

            const double incomeRate  = 1.0;
            long installedTicks = System.DateTime.UtcNow.Ticks - System.TimeSpan.TicksPerSecond * 5;
            device.ActiveMalware = new Malware
            {
                Type                = MalwareType.Miner,
                DeviceIp            = device.Ip,
                IncomeRate          = incomeRate,
                Currency            = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = installedTicks
            };

            player.LastSaveUtcTicks = installedTicks - System.TimeSpan.TicksPerSecond * 2;

            // --- Act ---
            var malwareList = new System.Collections.Generic.List<Malware> { device.ActiveMalware };
            var gains = OfflineIncomeCalculator.Calculate(
                player.LastSaveUtcTicks,
                malwareList,
                System.DateTime.UtcNow.Ticks);

            for (int i = 0; i < gains.Count; i++)
                player.AddBalance(gains[i].Currency, gains[i].Amount);

            // --- Assert ---
            double balance = player.GetBalance(CurrencyType.Bitcoin);
            Assert.GreaterOrEqual(balance, 4.0,
                $"Offline income should credit ~5 BTC (got {balance:F4}).");
        }
    }
}
