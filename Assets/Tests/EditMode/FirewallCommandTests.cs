using NUnit.Framework;
using UnityEngine;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;
using HackYourWay.Data;

namespace HackYourWay.Tests.EditMode
{
    public class FirewallCommandTests
    {
        private static Services.LocationService BuildLocationService(
            string ip             = "192.168.1.10",
            FirewallStatus fw     = FirewallStatus.Active,
            bool isScanned        = true)
        {
            var config = ScriptableObject.CreateInstance<LocationConfigSO>();
            config.MinNetworks          = 1;
            config.MaxNetworks          = 1;
            config.MinDevicesPerNetwork = 1;
            config.MaxDevicesPerNetwork = 1;
            config.MinFilesPerDevice    = 0;
            config.MaxFilesPerDevice    = 0;
            config.SecurityDistribution = new float[] { 0f, 0f, 0f, 1f };
            config.MinRansomAmount      = 0.01f;
            config.MaxRansomAmount      = 0.10f;

            var svc = new Services.LocationService(config);
            // Trigger location generation and mutate the device's firewall status + IP
            var loc = svc.GetCurrentLocation();
            var dev = loc.Networks[0].Devices[0];
            dev.Ip            = ip;
            dev.FirewallStatus = fw;
            dev.IsScanned     = isScanned;
            return svc;
        }

        private static (Player, FirewallCommand) MakeCmd(
            Services.LocationService svc, bool hasTool = true)
        {
            var player = new Player();
            if (hasTool) player.UnlockedTools.Add(ToolType.FirewallDisable);
            return (player, new FirewallCommand(player, svc));
        }

        [Test]
        public void Firewall_DisableWithIp_ActiveDevice_Succeeds()
        {
            var svc = BuildLocationService(fw: FirewallStatus.Active);
            var (_, cmd) = MakeCmd(svc);

            var result = cmd.Execute(new[] { "disable", "192.168.1.10" });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(FirewallStatus.Disabled,
                svc.GetCurrentLocation().Networks[0].Devices[0].FirewallStatus);
        }

        [Test]
        public void Firewall_EnableWithIp_DisabledDevice_Succeeds()
        {
            var svc = BuildLocationService(fw: FirewallStatus.Disabled);
            var (_, cmd) = MakeCmd(svc);

            var result = cmd.Execute(new[] { "enable", "192.168.1.10" });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(FirewallStatus.Active,
                svc.GetCurrentLocation().Networks[0].Devices[0].FirewallStatus);
        }

        [Test]
        public void Firewall_DisableAlreadyDisabled_ReturnsError()
        {
            var svc = BuildLocationService(fw: FirewallStatus.Disabled);
            var (_, cmd) = MakeCmd(svc);

            var result = cmd.Execute(new[] { "disable", "192.168.1.10" });

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Error:", result.Message);
        }

        [Test]
        public void Firewall_UnknownIp_ReturnsError()
        {
            var svc = BuildLocationService();
            var (_, cmd) = MakeCmd(svc);

            var result = cmd.Execute(new[] { "disable", "10.0.0.99" });

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Error:", result.Message);
        }

        [Test]
        public void Firewall_NoTool_ReturnsError()
        {
            var svc = BuildLocationService();
            var (_, cmd) = MakeCmd(svc, hasTool: false);

            var result = cmd.Execute(new[] { "disable", "192.168.1.10" });

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Error:", result.Message);
        }

        [Test]
        public void Firewall_InvalidSubCommand_ReturnsError()
        {
            var svc = BuildLocationService();
            var (_, cmd) = MakeCmd(svc);

            var result = cmd.Execute(new[] { "freeze", "192.168.1.10" });

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Error:", result.Message);
        }
    }
}
