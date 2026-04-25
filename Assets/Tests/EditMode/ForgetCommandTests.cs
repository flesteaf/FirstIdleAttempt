using System.Collections.Generic;
using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;

namespace HackYourWay.Tests.EditMode
{
    public class ForgetCommandTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static LocationService BuildSingleLocation(string ssid = "TestNet")
        {
            var net = new NetworkSaveData { Ssid = ssid, SecurityLevel = SecurityLevel.WPA2 };
            net.Devices.Add(new DeviceSaveData
            {
                Ip         = "192.168.1.10",
                HasMalware = true,
                ActiveMalware = new Malware { Type = MalwareType.Miner, IncomeRate = 0.001, Currency = CurrencyType.Bitcoin }
            });
            var loc = new LocationSaveData { Id = "1", Name = "node_77" };
            loc.Networks.Add(net);

            var svc = new LocationService(null);
            svc.RestoreFromSave(new List<LocationSaveData> { loc }, 2);
            return svc;
        }

        private static LocationService BuildTwoLocationsWithSharedSsid()
        {
            var svc = new LocationService(null);
            var saved = new List<LocationSaveData>
            {
                new LocationSaveData
                {
                    Id = "1", Name = "node_77",
                    Networks = new List<NetworkSaveData>
                    {
                        new NetworkSaveData { Ssid = "SharedNet", SecurityLevel = SecurityLevel.WPA2 }
                    }
                },
                new LocationSaveData
                {
                    Id = "2", Name = "node_154",
                    Networks = new List<NetworkSaveData>
                    {
                        new NetworkSaveData { Ssid = "SharedNet", SecurityLevel = SecurityLevel.WPA2 }
                    }
                }
            };
            svc.RestoreFromSave(saved, 3);
            return svc;
        }

        // ── forget network ────────────────────────────────────────────────────

        [Test]
        public void ForgetNetwork_SingleSsid_Succeeds()
        {
            var svc    = BuildSingleLocation();
            var result = new ForgetCommand(svc).Execute(new[] { "network", "TestNet" });

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void ForgetNetwork_SingleSsid_NetworkRemoved()
        {
            var svc = BuildSingleLocation();
            new ForgetCommand(svc).Execute(new[] { "network", "TestNet" });

            svc.SetCurrentLocation("node_77");
            Assert.AreEqual(0, svc.GetCurrentLocation().Networks.Count);
        }

        [Test]
        public void ForgetNetwork_UnknownSsid_ReturnsFail()
        {
            var svc    = BuildSingleLocation();
            var result = new ForgetCommand(svc).Execute(new[] { "network", "NoSuchNet" });

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void ForgetNetwork_AmbiguousSsid_ReturnsOkAndEntersSelection()
        {
            // Without TerminalController.Instance, AwaitSelection is a no-op.
            // The command should still return Ok (entering selection mode).
            var svc    = BuildTwoLocationsWithSharedSsid();
            var result = new ForgetCommand(svc).Execute(new[] { "network", "SharedNet" });

            // Ambiguous → enters interactive mode → returns Ok("")
            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.Message);
        }

        [Test]
        public void ForgetNetwork_AtLocation_ScopedRemoval()
        {
            var svc    = BuildTwoLocationsWithSharedSsid();
            var result = new ForgetCommand(svc)
                .Execute(new[] { "network", "SharedNet", "at", "node_77" });

            Assert.IsTrue(result.Success);
            // node_154 still has its SharedNet
            svc.SetCurrentLocation("node_154");
            Assert.AreEqual(1, svc.GetCurrentLocation().Networks.Count);
        }

        // ── forget ip ────────────────────────────────────────────────────────

        [Test]
        public void ForgetIp_KnownIp_Succeeds()
        {
            var svc    = BuildSingleLocation();
            var result = new ForgetCommand(svc).Execute(new[] { "ip", "192.168.1.10" });

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void ForgetIp_KnownIp_DeviceRemoved()
        {
            var svc = BuildSingleLocation();
            new ForgetCommand(svc).Execute(new[] { "ip", "192.168.1.10" });

            svc.SetCurrentLocation("node_77");
            Assert.AreEqual(0, svc.GetCurrentLocation().Networks[0].Devices.Count);
        }

        [Test]
        public void ForgetIp_UnknownIp_ReturnsFail()
        {
            var svc    = BuildSingleLocation();
            var result = new ForgetCommand(svc).Execute(new[] { "ip", "10.0.0.99" });

            Assert.IsFalse(result.Success);
        }

        // ── invalid syntax ────────────────────────────────────────────────────

        [Test]
        public void ForgetCommand_TooFewArgs_ReturnsUsageError()
        {
            var svc    = BuildSingleLocation();
            var result = new ForgetCommand(svc).Execute(new[] { "network" }); // missing SSID

            Assert.IsFalse(result.Success);
            StringAssert.Contains("Usage:", result.Message);
        }

        [Test]
        public void ForgetCommand_UnknownSubcommand_ReturnsUsageError()
        {
            var svc    = BuildSingleLocation();
            var result = new ForgetCommand(svc).Execute(new[] { "delete", "TestNet" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("Usage:", result.Message);
        }

        [Test]
        public void ForgetCommand_NoArgs_ReturnsUsageError()
        {
            var svc    = BuildSingleLocation();
            var result = new ForgetCommand(svc).Execute(new string[0]);

            Assert.IsFalse(result.Success);
            StringAssert.Contains("Usage:", result.Message);
        }
    }
}
