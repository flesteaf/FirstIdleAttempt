using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services.Commands;

namespace HackYourWay.Tests.EditMode
{
    public class FirewallCommandTests
    {
        private static (Player, Device) MakePlayerWithDevice(
            FirewallStatus fw = FirewallStatus.Active,
            bool hasTool = true)
        {
            var player = new Player();
            if (hasTool) player.UnlockedTools.Add(ToolType.FirewallDisable);

            var device = new Device { Ip = "192.168.1.10", FirewallStatus = fw };
            player.TargetedDevice = device;
            player.TargetedNetwork = new Network { Ssid = "TestNet", SecurityLevel = SecurityLevel.WPA2 };

            return (player, device);
        }

        [Test]
        public void Firewall_DisableOnActive_Succeeds()
        {
            var (player, device) = MakePlayerWithDevice(FirewallStatus.Active);
            var cmd    = new FirewallCommand(player);
            var result = cmd.Execute(new[] { "disable" });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(FirewallStatus.Disabled, device.FirewallStatus);
        }

        [Test]
        public void Firewall_EnableOnDisabled_Succeeds()
        {
            var (player, device) = MakePlayerWithDevice(FirewallStatus.Disabled);
            var cmd    = new FirewallCommand(player);
            var result = cmd.Execute(new[] { "enable" });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(FirewallStatus.Active, device.FirewallStatus);
        }

        [Test]
        public void Firewall_DisableOnAlreadyDisabled_ReturnsError()
        {
            var (player, _) = MakePlayerWithDevice(FirewallStatus.Disabled);
            var cmd    = new FirewallCommand(player);
            var result = cmd.Execute(new[] { "disable" });

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Error:", result.Message);
        }

        [Test]
        public void Firewall_NoTargetedDevice_ReturnsError()
        {
            var player = new Player();
            player.UnlockedTools.Add(ToolType.FirewallDisable);
            // TargetedDevice is null

            var cmd    = new FirewallCommand(player);
            var result = cmd.Execute(new[] { "disable" });

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Error:", result.Message);
        }

        [Test]
        public void Firewall_NoTool_ReturnsError()
        {
            var (player, _) = MakePlayerWithDevice(hasTool: false);
            var cmd    = new FirewallCommand(player);
            var result = cmd.Execute(new[] { "disable" });

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Error:", result.Message);
        }
    }
}
