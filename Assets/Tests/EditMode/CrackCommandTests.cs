using NUnit.Framework;
using UnityEngine;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;
using HackYourWay.Data;

namespace HackYourWay.Tests.EditMode
{
    public class CrackCommandTests
    {
        // Build a LocationService whose seed-1 location always produces WPA2 networks.
        private static Services.LocationService BuildLocationService()
        {
            var config = ScriptableObject.CreateInstance<LocationConfigSO>();
            config.MinNetworks = 1;
            config.MaxNetworks = 1;
            config.MinDevicesPerNetwork = 1;
            config.MaxDevicesPerNetwork = 1;
            config.MinFilesPerDevice = 0;
            config.MaxFilesPerDevice = 0;
            config.SecurityDistribution = new float[] { 0f, 0f, 0f, 1f }; // always WPA2
            config.MinRansomAmount = 0.01f;
            config.MaxRansomAmount = 0.10f;
            return new Services.LocationService(config);
        }

        [Test]
        public void Crack_CorrectToolAndLevel_GrantsAccess()
        {
            var svc     = BuildLocationService();
            var player  = new Player();
            player.UnlockedTools.Add(ToolType.CrackWPA2);
            var network = svc.GetCurrentLocation().Networks[0];

            var cmd    = new CrackCommand(player, svc);
            var result = cmd.Execute(new[] { "WPA2", network.Ssid });

            Assert.IsTrue(result.Success);
            Assert.IsTrue(network.IsHacked);
        }

        [Test]
        public void Crack_WrongTypeArgVsActualLevel_ReturnsMismatchError()
        {
            // Network is WPA2 but player declares WEP in the command argument.
            var svc     = BuildLocationService();
            var player  = new Player();
            player.UnlockedTools.Add(ToolType.CrackWEP);
            var network = svc.GetCurrentLocation().Networks[0]; // WPA2

            var cmd    = new CrackCommand(player, svc);
            var result = cmd.Execute(new[] { "WEP", network.Ssid }); // wrong type declared

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Error:", result.Message);
        }

        [Test]
        public void Crack_MissingTool_ReturnsPurchaseRequiredError()
        {
            var svc     = BuildLocationService();
            var player  = new Player(); // no tools
            var network = svc.GetCurrentLocation().Networks[0];

            var cmd    = new CrackCommand(player, svc);
            var result = cmd.Execute(new[] { "WPA2", network.Ssid });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("store", result.Message);
        }

        [Test]
        public void Crack_AlreadyHacked_ReturnsError()
        {
            var svc     = BuildLocationService();
            var player  = new Player();
            player.UnlockedTools.Add(ToolType.CrackWPA2);
            var network  = svc.GetCurrentLocation().Networks[0];
            network.IsHacked = true; // pre-hacked

            var cmd    = new CrackCommand(player, svc);
            var result = cmd.Execute(new[] { "WPA2", network.Ssid });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("already hacked", result.Message);
        }
    }
}
