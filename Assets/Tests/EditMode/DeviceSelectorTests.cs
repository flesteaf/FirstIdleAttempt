using NUnit.Framework;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;
using HackYourWay.Data;

namespace HackYourWay.Tests.EditMode
{
    public class DeviceSelectorTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static LocationService BuildService(int scannedDeviceCount)
        {
            var config = new LocationConfigSO();
            config.MinNetworks          = 1;
            config.MaxNetworks          = 1;
            config.MinDevicesPerNetwork = scannedDeviceCount;
            config.MaxDevicesPerNetwork = scannedDeviceCount;
            config.MinFilesPerDevice    = 0;
            config.MaxFilesPerDevice    = 0;
            config.SecurityDistribution = new float[] { 0f, 0f, 0f, 1f };
            config.MinRansomAmount      = 0.01f;
            config.MaxRansomAmount      = 0.10f;

            var svc = new LocationService(config);
            // GetCurrentLocation() seeds the location; mark all devices as scanned
            var loc = svc.GetCurrentLocation();
            for (int n = 0; n < loc.Networks.Count; n++)
                for (int d = 0; d < loc.Networks[n].Devices.Count; d++)
                    loc.Networks[n].Devices[d].IsScanned = true;
            return svc;
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Test]
        public void AwaitDevice_NoScannedDevices_ReturnsErrorString()
        {
            var svc = BuildService(scannedDeviceCount: 0);
            // Mark all as NOT scanned
            var loc = svc.GetCurrentLocation();
            for (int n = 0; n < loc.Networks.Count; n++)
                for (int d = 0; d < loc.Networks[n].Devices.Count; d++)
                    loc.Networks[n].Devices[d].IsScanned = false;

            string error = DeviceSelector.AwaitDevice(svc, _ => { });

            Assert.IsNotNull(error);
            StringAssert.Contains("scan", error);
        }

        [Test]
        public void AwaitDevice_NoScannedDevices_DoesNotInvokeCallback()
        {
            var svc = BuildService(scannedDeviceCount: 1);
            var loc = svc.GetCurrentLocation();
            for (int n = 0; n < loc.Networks.Count; n++)
                for (int d = 0; d < loc.Networks[n].Devices.Count; d++)
                    loc.Networks[n].Devices[d].IsScanned = false;

            bool callbackInvoked = false;
            DeviceSelector.AwaitDevice(svc, _ => callbackInvoked = true);

            Assert.IsFalse(callbackInvoked, "Callback should not fire when no devices are scanned.");
        }

        [Test]
        public void AwaitDevice_WithScannedDevices_ReturnsNull()
        {
            // TerminalController.Instance is null in EditMode — AwaitSelection becomes a no-op
            var svc = BuildService(scannedDeviceCount: 2);

            string error = DeviceSelector.AwaitDevice(svc, _ => { });

            Assert.IsNull(error, "Should return null (no error) when devices are available.");
        }

        [Test]
        public void AwaitDevice_EmptyLocation_ReturnsError()
        {
            var config = new LocationConfigSO();
            config.MinNetworks          = 0;
            config.MaxNetworks          = 0;
            config.MinDevicesPerNetwork = 0;
            config.MaxDevicesPerNetwork = 0;
            config.MinFilesPerDevice    = 0;
            config.MaxFilesPerDevice    = 0;
            config.SecurityDistribution = new float[] { 0f, 0f, 0f, 1f };
            config.MinRansomAmount      = 0.01f;
            config.MaxRansomAmount      = 0.10f;
            var svc = new LocationService(config);

            string error = DeviceSelector.AwaitDevice(svc, _ => { });

            Assert.IsNotNull(error);
        }
    }
}
