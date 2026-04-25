using NUnit.Framework;
using UnityEngine;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Data;

namespace HackYourWay.Tests.EditMode
{
    public class IncomeServiceTests
    {
        private static Services.LocationService BuildLocationService()
        {
            var config = ScriptableObject.CreateInstance<LocationConfigSO>();
            config.MinNetworks = 1;
            config.MaxNetworks = 1;
            config.MinDevicesPerNetwork = 2;
            config.MaxDevicesPerNetwork = 2;
            config.MinFilesPerDevice = 0;
            config.MaxFilesPerDevice = 0;
            config.SecurityDistribution = new float[] { 0f, 0f, 0f, 1f };
            config.MinRansomAmount = 0.01f;
            config.MaxRansomAmount = 0.10f;
            return new Services.LocationService(config);
        }

        [Test]
        public void OnTick_SingleMiner_AddsCorrectIncome()
        {
            var svc    = BuildLocationService();
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 0);

            // Inject a miner onto the first device.
            var device = svc.GetCurrentLocation().Networks[0].Devices[0];
            device.ActiveMalware = new Malware
            {
                Type               = MalwareType.Miner,
                IncomeRate         = 2.0,
                Currency           = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks - 1
            };

            var service = new IncomeService(player, svc);
            service.OnTick(1.0); // 1 second tick, rate=2 → +2 BTC

            Assert.AreEqual(2.0, player.GetBalance(CurrencyType.Bitcoin), delta: 0.0001);
        }

        [Test]
        public void OnTick_MultipleMiners_AccumulatesAll()
        {
            var svc    = BuildLocationService();
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 0);

            var devices = svc.GetCurrentLocation().Networks[0].Devices;
            // Two devices with miners at different rates.
            devices[0].ActiveMalware = new Malware
            {
                Type       = MalwareType.Miner,
                IncomeRate = 1.0,
                Currency   = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks - 1
            };
            devices[1].ActiveMalware = new Malware
            {
                Type       = MalwareType.Miner,
                IncomeRate = 3.0,
                Currency   = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks - 1
            };

            var service = new IncomeService(player, svc);
            service.OnTick(1.0); // 1s × (1 + 3) = 4 BTC

            Assert.AreEqual(4.0, player.GetBalance(CurrencyType.Bitcoin), delta: 0.0001);
        }

        [Test]
        public void OnTick_NoMiners_BalanceUnchanged()
        {
            var svc    = BuildLocationService();
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 10.0);

            // No malware installed.
            var service = new IncomeService(player, svc);
            service.OnTick(1.0);

            Assert.AreEqual(10.0, player.GetBalance(CurrencyType.Bitcoin), delta: 0.0001);
        }

        [Test]
        public void OnTick_MinerWithZeroRate_NoIncome()
        {
            var svc    = BuildLocationService();
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 0);

            var device = svc.GetCurrentLocation().Networks[0].Devices[0];
            device.ActiveMalware = new Malware
            {
                Type       = MalwareType.Miner,
                IncomeRate = 0.0,
                Currency   = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks - 1
            };

            var service = new IncomeService(player, svc);
            service.OnTick(1.0);

            Assert.AreEqual(0.0, player.GetBalance(CurrencyType.Bitcoin), delta: 0.0001);
        }

        [Test]
        public void OnTick_DevicesInTwoLocations_CombinesIncome()
        {
            var config = ScriptableObject.CreateInstance<LocationConfigSO>();
            config.MinNetworks = 1; config.MaxNetworks = 1;
            config.MinDevicesPerNetwork = 1; config.MaxDevicesPerNetwork = 1;
            config.MinFilesPerDevice = 0; config.MaxFilesPerDevice = 0;
            config.SecurityDistribution = new float[] { 0f, 0f, 0f, 1f };
            config.MinRansomAmount = 0.01f; config.MaxRansomAmount = 0.10f;

            var svc = new Services.LocationService(config);
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.AddBalance(CurrencyType.Bitcoin, 0);

            // Location A — move there and infect device
            svc.MoveToNextLocation();
            var devA = svc.GetCurrentLocation().Networks[0].Devices[0];
            devA.ActiveMalware = new Malware
            {
                Type = MalwareType.Miner, IncomeRate = 1.0,
                Currency = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks - 1
            };

            // Location B — move there and infect device
            svc.MoveToNextLocation();
            var devB = svc.GetCurrentLocation().Networks[0].Devices[0];
            devB.ActiveMalware = new Malware
            {
                Type = MalwareType.Spammer, IncomeRate = 3.0,
                Currency = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks - 1
            };

            var service = new IncomeService(player, svc);
            service.OnTick(1.0); // 1s × (1.0 + 3.0) = 4.0 BTC

            Assert.AreEqual(4.0, player.GetBalance(CurrencyType.Bitcoin), delta: 0.0001);
        }

        [Test]
        public void OnTick_CorrectCurrencyCredited()
        {
            var svc    = BuildLocationService();
            var player = new Player();
            player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            player.UnlockedCurrencies.Add(CurrencyType.Monero);
            player.AddBalance(CurrencyType.Bitcoin, 0);
            player.AddBalance(CurrencyType.Monero, 0);

            var device = svc.GetCurrentLocation().Networks[0].Devices[0];
            device.ActiveMalware = new Malware
            {
                Type       = MalwareType.Miner,
                IncomeRate = 5.0,
                Currency   = CurrencyType.Monero,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks - 1
            };

            var service = new IncomeService(player, svc);
            service.OnTick(1.0);

            Assert.AreEqual(0.0, player.GetBalance(CurrencyType.Bitcoin), delta: 0.0001);
            Assert.AreEqual(5.0, player.GetBalance(CurrencyType.Monero), delta: 0.0001);
        }
    }
}
