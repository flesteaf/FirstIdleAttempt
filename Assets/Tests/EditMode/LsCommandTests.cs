using NUnit.Framework;
using UnityEngine;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;
using HackYourWay.Data;

namespace HackYourWay.Tests.EditMode
{
    public class LsCommandTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static LocationService BuildService(string ip = "10.0.0.1", bool scanned = true)
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

            var svc = new LocationService(config);
            var dev = svc.GetCurrentLocation().Networks[0].Devices[0];
            dev.Ip        = ip;
            dev.IsScanned = scanned;
            return svc;
        }

        private static LocationService BuildServiceWithFiles(string ip = "10.0.0.1")
        {
            var svc = BuildService(ip, scanned: true);
            var dev = svc.GetCurrentLocation().Networks[0].Devices[0];
            dev.Files.Add(new DeviceFile { Name = "notes.txt", Path = "/home/notes.txt", SizeBytes = 1024 });
            dev.Files.Add(new DeviceFile { Name = "secret.key", Path = "/root/secret.key", SizeBytes = 512 });
            return svc;
        }

        // ── Direct IP path ────────────────────────────────────────────────────

        [Test]
        public void Ls_ExplicitIp_NoFiles_ReturnsNoFilesMessage()
        {
            var svc = BuildService();
            var result = new LsCommand(svc).Execute(new[] { "10.0.0.1" });

            Assert.IsTrue(result.Success);
            StringAssert.Contains("no files found", result.Message);
        }

        [Test]
        public void Ls_ExplicitIp_WithFiles_ListsFilePaths()
        {
            var svc = BuildServiceWithFiles();
            var result = new LsCommand(svc).Execute(new[] { "10.0.0.1" });

            Assert.IsTrue(result.Success);
            StringAssert.Contains("notes.txt", result.Message);
            StringAssert.Contains("secret.key", result.Message);
        }

        [Test]
        public void Ls_ExplicitIp_UnknownDevice_ReturnsError()
        {
            var svc = BuildService();
            var result = new LsCommand(svc).Execute(new[] { "99.99.99.99" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("not found", result.Message);
        }

        // ── Interactive selection path ─────────────────────────────────────────

        [Test]
        public void Ls_NoIp_NoScannedDevices_ReturnsError()
        {
            var svc = BuildService(scanned: false);
            var result = new LsCommand(svc).Execute(new string[0]);

            Assert.IsFalse(result.Success);
            StringAssert.Contains("scan", result.Message);
        }

        [Test]
        public void Ls_NoIp_WithScannedDevice_ReturnsOk()
        {
            // TerminalController.Instance is null in EditMode — AwaitSelection becomes no-op
            var svc = BuildService(scanned: true);
            var result = new LsCommand(svc).Execute(new string[0]);

            Assert.IsTrue(result.Success);
        }
    }
}
