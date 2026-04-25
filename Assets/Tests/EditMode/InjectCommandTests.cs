using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;

namespace HackYourWay.Tests.EditMode
{
    /// <summary>
    /// Tests for InjectCommand direct-inject path:
    /// inject {type} {IP} {SSID} without requiring a prior scan ip.
    /// </summary>
    public class InjectCommandDirectInjectTests
    {
        // ── Helper ────────────────────────────────────────────────────────────

        private static (Player player, Device device, Network network, StubLocationService ls)
            MakeDirectSetup(
                FirewallStatus fw              = FirewallStatus.Disabled,
                bool           alreadyInfected = false,
                SecurityLevel  secLevel        = SecurityLevel.WPA2,
                bool           isHacked        = true,
                string         ip              = "192.168.1.20",
                string         ssid            = "DirectNet")
        {
            var device = new Device { Ip = ip, FirewallStatus = fw };
            if (alreadyInfected)
                device.ActiveMalware = new Malware { Type = MalwareType.Miner, IncomeRate = 0.001 };

            var network = new Network { Ssid = ssid, SecurityLevel = secLevel, IsHacked = isHacked };
            network.Devices.Add(device);

            var location = new Location { Id = "stub" };
            location.Networks.Add(network);

            var ls     = new StubLocationService(location);
            var player = new Player();

            return (player, device, network, ls);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Test]
        public void DirectInject_ValidIpAndSsid_MinerInstalled()
        {
            var (player, device, _, ls) = MakeDirectSetup();
            var result = new InjectCommand(player, ls)
                .Execute(new[] { "miner", "192.168.1.20", "DirectNet" });

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(device.ActiveMalware);
            Assert.AreEqual(MalwareType.Miner, device.ActiveMalware.Type);
        }

        [Test]
        public void DirectInject_NetworkNotFound_ReturnsError()
        {
            var (player, _, _, ls) = MakeDirectSetup();
            var result = new InjectCommand(player, ls)
                .Execute(new[] { "miner", "192.168.1.20", "NoSuchNet" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("not found", result.Message);
        }

        [Test]
        public void DirectInject_NetworkNotAccessible_ReturnsError()
        {
            var (player, _, _, ls) = MakeDirectSetup(secLevel: SecurityLevel.WPA2, isHacked: false);
            var result = new InjectCommand(player, ls)
                .Execute(new[] { "miner", "192.168.1.20", "DirectNet" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("not accessible", result.Message);
        }

        [Test]
        public void DirectInject_OpenNetwork_BypassesCrackRequirement()
        {
            // SecurityLevel.None + IsHacked=false: open network waiver applies.
            var (player, device, _, ls) =
                MakeDirectSetup(secLevel: SecurityLevel.None, isHacked: false);
            var result = new InjectCommand(player, ls)
                .Execute(new[] { "miner", "192.168.1.20", "DirectNet" });

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(device.ActiveMalware);
        }

        [Test]
        public void DirectInject_DeviceNotOnNetwork_ReturnsError()
        {
            var (player, _, _, ls) = MakeDirectSetup();
            var result = new InjectCommand(player, ls)
                .Execute(new[] { "miner", "10.0.0.99", "DirectNet" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("not found on network", result.Message);
        }

        [Test]
        public void DirectInject_FirewallActive_ReturnsError()
        {
            var (player, _, _, ls) = MakeDirectSetup(fw: FirewallStatus.Active);
            var result = new InjectCommand(player, ls)
                .Execute(new[] { "miner", "192.168.1.20", "DirectNet" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("Firewall", result.Message);
        }

        [Test]
        public void DirectInject_AlreadyInfected_ReturnsError()
        {
            var (player, _, _, ls) = MakeDirectSetup(alreadyInfected: true);
            var result = new InjectCommand(player, ls)
                .Execute(new[] { "miner", "192.168.1.20", "DirectNet" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("already infected", result.Message);
        }

        [Test]
        public void DirectInject_PartialArgs_OnlyIp_ReturnsUsageError()
        {
            var (player, _, _, ls) = MakeDirectSetup();
            // Two args total: type + IP only (no SSID).
            var result = new InjectCommand(player, ls)
                .Execute(new[] { "miner", "192.168.1.20" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("Usage:", result.Message);
        }

        // ── Stub ──────────────────────────────────────────────────────────────

        private class StubLocationService : LocationService
        {
            private readonly Location _location;

            public StubLocationService(Location location) : base(null)
            {
                _location = location;
            }

            public override Location GetCurrentLocation() => _location;
        }
    }
}
