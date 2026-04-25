using NUnit.Framework;
using UnityEngine;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;
using HackYourWay.Data;

namespace HackYourWay.Tests.EditMode
{
    public class CopyCommandTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static (Player, CopyCommand, LocationService) Make(
            string ip      = "10.0.0.1",
            bool scanned   = true,
            bool withFiles = false)
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

            if (withFiles)
                dev.Files.Add(new DeviceFile
                {
                    Name      = "report.pdf",
                    Path      = "/docs/report.pdf",
                    SizeBytes = 4096
                });

            var player = new Player();
            return (player, new CopyCommand(player, svc), svc);
        }

        // ── Argument validation ───────────────────────────────────────────────

        [Test]
        public void Copy_NoArgs_ReturnsUsageError()
        {
            var (_, cmd, _) = Make();
            var result = cmd.Execute(new string[0]);

            Assert.IsFalse(result.Success);
            StringAssert.Contains("Usage:", result.Message);
        }

        // ── Direct IP path ────────────────────────────────────────────────────

        [Test]
        public void Copy_ExplicitIpAndFile_CopiesSuccessfully()
        {
            var (player, cmd, _) = Make(withFiles: true);
            var result = cmd.Execute(new[] { "report.pdf", "10.0.0.1" });

            Assert.IsTrue(result.Success);
            Assert.IsTrue(player.CopiedFiles.Contains("/docs/report.pdf"));
        }

        [Test]
        public void Copy_ExplicitIp_FileNotFound_ReturnsError()
        {
            var (_, cmd, _) = Make(withFiles: false);
            var result = cmd.Execute(new[] { "missing.txt", "10.0.0.1" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("not found", result.Message);
        }

        [Test]
        public void Copy_ExplicitIp_UnknownDevice_ReturnsError()
        {
            var (_, cmd, _) = Make();
            var result = cmd.Execute(new[] { "report.pdf", "99.99.99.99" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("not found", result.Message);
        }

        [Test]
        public void Copy_SameFile_Twice_AddedOnce()
        {
            var (player, cmd, _) = Make(withFiles: true);
            cmd.Execute(new[] { "report.pdf", "10.0.0.1" });
            cmd.Execute(new[] { "report.pdf", "10.0.0.1" });

            int count = 0;
            foreach (var f in player.CopiedFiles)
                if (f == "/docs/report.pdf") count++;
            Assert.AreEqual(1, count);
        }

        // ── Interactive selection path ─────────────────────────────────────────

        [Test]
        public void Copy_NoIp_NoScannedDevices_ReturnsError()
        {
            var (_, cmd, _) = Make(scanned: false);
            var result = cmd.Execute(new[] { "report.pdf" });

            Assert.IsFalse(result.Success);
            StringAssert.Contains("scan", result.Message);
        }

        [Test]
        public void Copy_NoIp_WithScannedDevice_ReturnsOk()
        {
            // TerminalController.Instance is null in EditMode — AwaitSelection becomes no-op
            var (_, cmd, _) = Make(scanned: true);
            var result = cmd.Execute(new[] { "report.pdf" });

            Assert.IsTrue(result.Success);
        }
    }
}
