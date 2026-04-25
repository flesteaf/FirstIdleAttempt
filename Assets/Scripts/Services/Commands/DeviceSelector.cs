using System;
using System.Collections.Generic;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Shared helper that presents a numbered list of scanned devices at the current location
    /// and calls <paramref name="onSelected"/> with the chosen device (or <c>null</c> on cancel).
    /// Used by <see cref="FirewallCommand"/>, <see cref="LsCommand"/>, <see cref="CopyCommand"/>.
    /// </summary>
    public static class DeviceSelector
    {
        /// <summary>
        /// Queries scanned devices at the current location and invokes interactive selection.
        /// If no devices are scanned, returns an error message without prompting.
        /// </summary>
        /// <param name="locationService">Used to retrieve scanned devices at the current location.</param>
        /// <param name="onSelected">Callback receiving the chosen <see cref="Device"/>, or <c>null</c> on cancel.</param>
        /// <returns>
        /// <c>null</c> when selection mode was successfully entered; a non-null error string when
        /// selection cannot proceed (e.g. no devices discovered).
        /// </returns>
        public static string AwaitDevice(LocationService locationService, Action<Device> onSelected)
        {
            List<Device> devices = locationService.GetScannedDevicesAtCurrentLocation();
            if (devices.Count == 0)
                return "No devices discovered at current location. Use 'scan' first.";

            string[] options = new string[devices.Count];
            for (int i = 0; i < devices.Count; i++)
                options[i] = devices[i].Ip;

            UI.TerminalController.Instance?.AwaitSelection(options, idx =>
            {
                onSelected(idx < 0 ? null : devices[idx]);
            });

            return null;
        }
    }
}
