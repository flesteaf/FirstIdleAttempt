using NUnit.Framework;
using UnityEngine;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;
using HackYourWay.Data;

namespace HackYourWay.Tests.EditMode
{
    public class ScanCommandTests
    {
        private static Services.LocationService BuildLocationService()
        {
            var config = ScriptableObject.CreateInstance<LocationConfigSO>();
            config.MinNetworks = 2;
            config.MaxNetworks = 2;
            config.MinDevicesPerNetwork = 1;
            config.MaxDevicesPerNetwork = 1;
            config.MinFilesPerDevice = 0;
            config.MaxFilesPerDevice = 0;
            config.SecurityDistribution = new float[] { 0f, 0f, 0f, 1f }; // always WPA2
            config.MinRansomAmount = 0.01f;
            config.MaxRansomAmount = 0.10f;
            var svc = new Services.LocationService(config);
            svc.MoveToNextLocation(); // populate cache so HasCurrentLocation() returns true
            return svc;
        }

        [Test]
        public void Scan_FirstCall_ReturnsNetworkList()
        {
            var svc    = BuildLocationService();
            var player = new Player();
            var cmd    = new ScanCommand(player, svc);

            var result = cmd.Execute(new string[0]);

            Assert.IsTrue(result.Success);
            StringAssert.Contains("Found", result.Message);
            StringAssert.Contains("network", result.Message);
        }

        [Test]
        public void Scan_SecondCall_DoesNotChangeLocation()
        {
            var svc    = BuildLocationService();
            var player = new Player();
            var cmd    = new ScanCommand(player, svc);

            string firstId = svc.GetCurrentLocation().Id;
            cmd.Execute(new string[0]);
            cmd.Execute(new string[0]); // second call — must NOT move location

            Assert.AreEqual(firstId, svc.GetCurrentLocation().Id);
        }

        [Test]
        public void ScanNetwork_KnownSsid_ReturnsSecurityAndDevices()
        {
            var svc    = BuildLocationService();
            var player = new Player();
            var cmd    = new ScanCommand(player, svc);

            cmd.Execute(new string[0]);
            string ssid = svc.GetCurrentLocation().Networks[0].Ssid;

            var result = cmd.Execute(new[] { "network", ssid });

            Assert.IsTrue(result.Success);
            StringAssert.Contains("Security:", result.Message);
            StringAssert.Contains("Devices found:", result.Message);
        }

        [Test]
        public void ScanIp_AccessibleDevice_ReturnsFirewallAndPorts()
        {
            var svc    = BuildLocationService();
            var player = new Player();
            var cmd    = new ScanCommand(player, svc);

            cmd.Execute(new string[0]);
            var location = svc.GetCurrentLocation();
            location.Networks[0].IsHacked = true;
            string ip = location.Networks[0].Devices[0].Ip;

            var result = cmd.Execute(new[] { "ip", ip });

            Assert.IsTrue(result.Success);
            StringAssert.Contains("Firewall:", result.Message);
            StringAssert.Contains("Open ports:", result.Message);
        }

        [Test]
        public void ScanNetwork_UnknownSsid_ReturnsError()
        {
            var svc    = BuildLocationService();
            var player = new Player();
            var cmd    = new ScanCommand(player, svc);

            cmd.Execute(new string[0]);

            var result = cmd.Execute(new[] { "network", "NoSuchNetwork_XYZ" });

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Error:", result.Message);
        }
    }
}
