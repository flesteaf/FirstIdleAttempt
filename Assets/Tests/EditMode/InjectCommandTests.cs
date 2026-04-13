using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services.Commands;

namespace HackYourWay.Tests.EditMode
{
    /// <summary>Tests for InjectCommand — miner injection only (Phase 3 / US1 scope).</summary>
    public class InjectCommandTests
    {
        private static (Player player, Device device, Network network) MakeSetup(
            FirewallStatus fw = FirewallStatus.Disabled,
            bool alreadyInfected = false,
            bool hasTarget = true,
            SecurityLevel secLevel = SecurityLevel.WPA2)
        {
            var player  = new Player();
            var network = new Network { Ssid = "TestNet", SecurityLevel = secLevel, IsHacked = true };
            var device  = new Device { Ip = "192.168.1.10", FirewallStatus = fw };
            network.Devices.Add(device);

            if (alreadyInfected)
                device.ActiveMalware = new Malware { Type = MalwareType.Miner, IncomeRate = 0.001 };

            if (hasTarget)
            {
                player.TargetedDevice  = device;
                player.TargetedNetwork = network;
            }

            return (player, device, network);
        }

        [Test]
        public void Inject_MinerOnAccessibleDevice_Succeeds()
        {
            var (player, device, _) = MakeSetup(fw: FirewallStatus.Disabled);
            var cmd    = new InjectCommand(player);
            var result = cmd.Execute(new[] { "miner" });

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(device.ActiveMalware);
            Assert.AreEqual(MalwareType.Miner, device.ActiveMalware.Type);
        }

        [Test]
        public void Inject_FirewallActive_BlocksInjection()
        {
            var (player, _, _) = MakeSetup(fw: FirewallStatus.Active);
            var cmd    = new InjectCommand(player);
            var result = cmd.Execute(new[] { "miner" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("Firewall", result.Message);
        }

        [Test]
        public void Inject_AlreadyInfected_ReturnsError()
        {
            var (player, _, _) = MakeSetup(alreadyInfected: true);
            var cmd    = new InjectCommand(player);
            var result = cmd.Execute(new[] { "miner" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("already infected", result.Message);
        }

        [Test]
        public void Inject_NoTargetedDevice_ReturnsError()
        {
            var (player, _, _) = MakeSetup(hasTarget: false);
            var cmd    = new InjectCommand(player);
            var result = cmd.Execute(new[] { "miner" });

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Error:", result.Message);
        }

        [Test]
        public void Inject_OpenNetwork_BypassesFirewallCheck()
        {
            // Open networks waive the firewall condition (spec assumption).
            var (player, device, _) = MakeSetup(fw: FirewallStatus.Active, secLevel: SecurityLevel.None);
            var cmd    = new InjectCommand(player);
            var result = cmd.Execute(new[] { "miner" });

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(device.ActiveMalware);
        }
    }
}
