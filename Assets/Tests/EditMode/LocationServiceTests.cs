using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Data;

namespace HackYourWay.Tests.EditMode
{
    public class LocationServiceTests
    {
        private static Services.LocationService BuildEmpty()
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
            return new Services.LocationService(config);
        }

        private static List<LocationSaveData> MakeSaveData(int count)
        {
            var list = new List<LocationSaveData>();
            for (int i = 1; i <= count; i++)
                list.Add(new LocationSaveData
                {
                    Id        = i.ToString(),
                    Name      = $"node_{i * 77 % 1000}",
                    ConfigId  = string.Empty,
                    IsVisited = false
                });
            return list;
        }

        // ── T006 / T008: RestoreFromSave ──────────────────────────────────────

        [Test]
        public void RestoreFromSave_ThreeLocations_AllKnown()
        {
            var svc = BuildEmpty();
            svc.RestoreFromSave(MakeSaveData(3), 4);
            Assert.AreEqual(3, svc.GetAllKnownLocations().Count);
        }

        [Test]
        public void GetAllKnownLocations_CalledTwice_ReturnsSameReference()
        {
            var svc = BuildEmpty();
            svc.RestoreFromSave(MakeSaveData(2), 3);
            var first  = svc.GetAllKnownLocations();
            var second = svc.GetAllKnownLocations();
            Assert.AreSame(first, second);
        }

        // ── T007: Name formula ────────────────────────────────────────────────

        [Test]
        public void GenerateLocation_Seed1_ProducesNode77()
        {
            var svc = BuildEmpty();
            svc.MoveToNextLocation(); // _currentLocationId starts at 1
            Assert.AreEqual("node_77", svc.GetCurrentLocationName());
        }

        [Test]
        public void GenerateLocation_Seed2_ProducesNode154()
        {
            var svc = BuildEmpty();
            svc.MoveToNextLocation(); // seed 1
            svc.MoveToNextLocation(); // seed 2
            Assert.AreEqual("node_154", svc.GetCurrentLocationName());
        }

        // ── T009: v1 save Name back-fill ──────────────────────────────────────

        [Test]
        public void RestoreFromSave_MissingName_BackfillsFromSeed()
        {
            var svc = BuildEmpty();
            var v1 = new List<LocationSaveData>
            {
                new LocationSaveData { Id = "1", Name = null, ConfigId = string.Empty }
            };
            svc.RestoreFromSave(v1, 2);
            Assert.IsTrue(svc.SetCurrentLocation("node_77"),
                "seed 1 (77 × 1 % 1000 = 77) should back-fill to 'node_77'");
        }

        [Test]
        public void RestoreFromSave_MissingNameId2_BackfillsNode154()
        {
            var svc = BuildEmpty();
            var v1 = new List<LocationSaveData>
            {
                new LocationSaveData { Id = "2", Name = null, ConfigId = string.Empty }
            };
            svc.RestoreFromSave(v1, 3);
            Assert.IsTrue(svc.SetCurrentLocation("node_154"),
                "seed 2 (77 × 2 % 1000 = 154) should back-fill to 'node_154'");
        }

        // ── T017: SetCurrentLocation ──────────────────────────────────────────

        [Test]
        public void SetCurrentLocation_KnownName_ReturnsTrue()
        {
            var svc = BuildEmpty();
            svc.RestoreFromSave(MakeSaveData(2), 3);
            Assert.IsTrue(svc.SetCurrentLocation("node_77"));
        }

        [Test]
        public void SetCurrentLocation_UnknownName_ReturnsFalse()
        {
            var svc = BuildEmpty();
            svc.RestoreFromSave(MakeSaveData(1), 2);
            Assert.IsFalse(svc.SetCurrentLocation("does_not_exist"));
        }

        [Test]
        public void SetCurrentLocation_UnknownName_LocationUnchanged()
        {
            var svc = BuildEmpty();
            svc.RestoreFromSave(MakeSaveData(1), 2);
            svc.SetCurrentLocation("node_77");              // set to known
            svc.SetCurrentLocation("does_not_exist");       // fails
            Assert.AreEqual("node_77", svc.GetCurrentLocationName());
        }

        // ── T018: GetCurrentLocationName ──────────────────────────────────────

        [Test]
        public void GetCurrentLocationName_AfterSetCurrentLocation_ReturnsCorrectName()
        {
            var svc = BuildEmpty();
            svc.RestoreFromSave(MakeSaveData(2), 3);
            svc.SetCurrentLocation("node_154"); // seed 2
            Assert.AreEqual("node_154", svc.GetCurrentLocationName());
        }

        // ── T033: ForgetNetwork ───────────────────────────────────────────────

        [Test]
        public void ForgetNetwork_NetworkWithInfectedDevices_CountsRemovedIps()
        {
            var svc = BuildEmpty();
            svc.MoveToNextLocation();
            var loc  = svc.GetCurrentLocation();
            string ssid = loc.Networks[0].Ssid;

            foreach (var dev in loc.Networks[0].Devices)
                dev.ActiveMalware = new Malware { Type = MalwareType.Miner };

            int infected = loc.Networks[0].Devices.Count;
            var result   = svc.ForgetNetwork(ssid);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(infected, result.IpsRemoved);
        }

        [Test]
        public void ForgetNetwork_UnknownSsid_ReturnsFail()
        {
            var svc = BuildEmpty();
            svc.MoveToNextLocation();
            var result = svc.ForgetNetwork("NoSuchNet_XYZ");
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void ForgetNetwork_AmbiguousSsid_ReturnsAmbiguous()
        {
            var svc = BuildEmpty();
            var saved = new List<LocationSaveData>
            {
                new LocationSaveData
                {
                    Id = "10", Name = "node_770",
                    Networks = new List<NetworkSaveData>
                    {
                        new NetworkSaveData { Ssid = "SharedNet", SecurityLevel = SecurityLevel.WPA2 }
                    }
                },
                new LocationSaveData
                {
                    Id = "11", Name = "node_847",
                    Networks = new List<NetworkSaveData>
                    {
                        new NetworkSaveData { Ssid = "SharedNet", SecurityLevel = SecurityLevel.WPA2 }
                    }
                }
            };
            svc.RestoreFromSave(saved, 12);

            var result = svc.ForgetNetwork("SharedNet");

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith("Network found at multiple locations:", result.Message);
        }

        [Test]
        public void ForgetNetwork_LocationFilter_NarrowsCorrectly()
        {
            var svc = BuildEmpty();
            var saved = new List<LocationSaveData>
            {
                new LocationSaveData
                {
                    Id = "10", Name = "node_770",
                    Networks = new List<NetworkSaveData>
                    {
                        new NetworkSaveData { Ssid = "SharedNet", SecurityLevel = SecurityLevel.WPA2 }
                    }
                },
                new LocationSaveData
                {
                    Id = "11", Name = "node_847",
                    Networks = new List<NetworkSaveData>
                    {
                        new NetworkSaveData { Ssid = "SharedNet", SecurityLevel = SecurityLevel.WPA2 }
                    }
                }
            };
            svc.RestoreFromSave(saved, 12);

            var result = svc.ForgetNetwork("SharedNet", "node_770");

            Assert.IsTrue(result.Success);
            // node_847 still has its SharedNet
            svc.SetCurrentLocation("node_847");
            Assert.AreEqual(1, svc.GetCurrentLocation().Networks.Count);
        }

        // ── T034: ForgetDevice ────────────────────────────────────────────────

        [Test]
        public void ForgetDevice_KnownInfectedIp_ReturnsSuccessWithCount()
        {
            var svc = BuildEmpty();
            svc.MoveToNextLocation();
            var dev = svc.GetCurrentLocation().Networks[0].Devices[0];
            dev.ActiveMalware = new Malware { Type = MalwareType.Miner };
            string ip = dev.Ip;

            var result = svc.ForgetDevice(ip);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.IpsRemoved);
        }

        [Test]
        public void ForgetDevice_KnownCleanIp_ReturnsSuccessZeroCount()
        {
            var svc = BuildEmpty();
            svc.MoveToNextLocation();
            string ip = svc.GetCurrentLocation().Networks[0].Devices[0].Ip;

            var result = svc.ForgetDevice(ip);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.IpsRemoved);
        }

        [Test]
        public void ForgetDevice_UnknownIp_ReturnsFail()
        {
            var svc = BuildEmpty();
            svc.MoveToNextLocation();
            var result = svc.ForgetDevice("10.0.0.99");
            Assert.IsFalse(result.Success);
        }
    }
}
