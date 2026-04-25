using System.Collections.Generic;
using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;

namespace HackYourWay.Tests.EditMode
{
    public class ShowCommandTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static LocationService BuildServiceWithLocations(int locationCount, int networksPerLocation)
        {
            var svc   = new LocationService(null);
            var saved = new List<LocationSaveData>();

            for (int l = 1; l <= locationCount; l++)
            {
                var loc = new LocationSaveData
                {
                    Id       = l.ToString(),
                    Name     = $"node_{l * 77 % 1000}",
                    ConfigId = string.Empty
                };
                for (int n = 0; n < networksPerLocation; n++)
                    loc.Networks.Add(new NetworkSaveData
                    {
                        Ssid          = $"Net_{l}_{n}",
                        SecurityLevel = SecurityLevel.WPA2
                    });
                saved.Add(loc);
            }

            svc.RestoreFromSave(saved, locationCount + 1);
            return svc;
        }

        private static LocationService BuildServiceWithInfectedDevice(
            string locationId = "1", string locationName = "node_77",
            string ssid = "TestNet", string ip = "192.168.1.10")
        {
            var dev = new DeviceSaveData
            {
                Ip            = ip,
                HasMalware    = true,
                ActiveMalware = new Malware { Type = MalwareType.Miner, IncomeRate = 0.0012, Currency = CurrencyType.Bitcoin }
            };
            var net = new NetworkSaveData { Ssid = ssid, SecurityLevel = SecurityLevel.WPA2 };
            net.Devices.Add(dev);
            var loc = new LocationSaveData { Id = locationId, Name = locationName };
            loc.Networks.Add(net);

            var svc = new LocationService(null);
            svc.RestoreFromSave(new List<LocationSaveData> { loc }, 2);
            return svc;
        }

        // ── show networks ─────────────────────────────────────────────────────

        [Test]
        public void ShowNetworks_TwoLocationsEachTwoNetworks_FourNetworkLines()
        {
            var svc    = BuildServiceWithLocations(2, 2);
            var cmd    = new ShowCommand(svc);
            var result = cmd.Execute(new[] { "networks" });

            Assert.IsTrue(result.Success);
            // Header line + 4 network lines (each location × 2 networks)
            int networkLines = 0;
            foreach (var line in result.Message.Split('\n'))
                if (line.Contains("Net_")) networkLines++;
            Assert.AreEqual(4, networkLines);
        }

        [Test]
        public void ShowNetworks_AlwaysRendersHeader()
        {
            var svc    = new LocationService(null);          // no locations
            var result = new ShowCommand(svc).Execute(new[] { "networks" });

            Assert.IsTrue(result.Success);
            StringAssert.Contains("NETWORK", result.Message);
            StringAssert.Contains("LOCATION", result.Message);
        }

        // ── show ips ──────────────────────────────────────────────────────────

        [Test]
        public void ShowIps_InfectedDeviceInTwoLocations_BothListed()
        {
            // Build two separate infected devices at two locations
            var svc = new LocationService(null);
            var saved = new List<LocationSaveData>
            {
                new LocationSaveData
                {
                    Id = "1", Name = "node_77",
                    Networks = new List<NetworkSaveData>
                    {
                        new NetworkSaveData
                        {
                            Ssid = "NetA", SecurityLevel = SecurityLevel.WPA2,
                            Devices = new List<DeviceSaveData>
                            {
                                new DeviceSaveData
                                {
                                    Ip = "192.168.1.10",
                                    HasMalware = true,
                                    ActiveMalware = new Malware { Type = MalwareType.Miner, IncomeRate = 0.001, Currency = CurrencyType.Bitcoin }
                                }
                            }
                        }
                    }
                },
                new LocationSaveData
                {
                    Id = "2", Name = "node_154",
                    Networks = new List<NetworkSaveData>
                    {
                        new NetworkSaveData
                        {
                            Ssid = "NetB", SecurityLevel = SecurityLevel.WPA2,
                            Devices = new List<DeviceSaveData>
                            {
                                new DeviceSaveData
                                {
                                    Ip = "10.0.0.5",
                                    HasMalware = true,
                                    ActiveMalware = new Malware { Type = MalwareType.Spammer, IncomeRate = 0.0008, Currency = CurrencyType.Bitcoin }
                                }
                            }
                        }
                    }
                }
            };
            svc.RestoreFromSave(saved, 3);

            var result = new ShowCommand(svc).Execute(new[] { "ips" });

            Assert.IsTrue(result.Success);
            StringAssert.Contains("192.168.1.10", result.Message);
            StringAssert.Contains("10.0.0.5", result.Message);
        }

        [Test]
        public void ShowIps_NoInfectedDevices_HeaderOnly()
        {
            var svc    = BuildServiceWithLocations(1, 2); // clean devices
            var result = new ShowCommand(svc).Execute(new[] { "ips" });

            Assert.IsTrue(result.Success);
            StringAssert.Contains("IP", result.Message);
            StringAssert.Contains("INCOME", result.Message);
            // No device IP in output
            Assert.IsFalse(result.Message.Contains("192.168."));
        }

        // ── show locations ────────────────────────────────────────────────────

        [Test]
        public void ShowLocations_ThreeKnownLocations_ThreeLines()
        {
            var svc = BuildServiceWithLocations(3, 1);
            svc.SetCurrentLocation("node_154"); // mark location 2 as current
            var result = new ShowCommand(svc).Execute(new[] { "locations" });

            Assert.IsTrue(result.Success);
            int locationLines = 0;
            foreach (var line in result.Message.Split('\n'))
                if (line.Contains("node_")) locationLines++;
            Assert.AreEqual(3, locationLines);
        }

        [Test]
        public void ShowLocations_CurrentLocationMarkedWithArrow()
        {
            var svc = BuildServiceWithLocations(2, 1);
            svc.SetCurrentLocation("node_154"); // seed 2
            var result = new ShowCommand(svc).Execute(new[] { "locations" });

            Assert.IsTrue(result.Success);
            // The line for node_154 must contain ">"
            bool found = false;
            foreach (var line in result.Message.Split('\n'))
                if (line.Contains("node_154") && line.Contains(">")) { found = true; break; }
            Assert.IsTrue(found, "Current location 'node_154' should be marked with '>'");
        }

        [Test]
        public void ShowLocations_AlwaysRendersHeader()
        {
            var svc    = new LocationService(null); // no locations
            var result = new ShowCommand(svc).Execute(new[] { "locations" });

            Assert.IsTrue(result.Success);
            StringAssert.Contains("LOCATION", result.Message);
            StringAssert.Contains("NETWORKS", result.Message);
        }
    }
}
