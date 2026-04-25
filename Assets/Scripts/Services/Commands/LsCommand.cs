using System.Text;
using HackYourWay.Interfaces;
using HackYourWay.Models;
using HackYourWay.Services;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>ls [{IP}]</c> — lists files on a device.
    /// If IP is provided the device is resolved directly; otherwise interactive device
    /// selection is presented via <see cref="UI.TerminalController.AwaitSelection"/>.
    /// </summary>
    public class LsCommand : ICommand
    {
        private readonly LocationService       _locationService;
        private readonly CommandLatencyService _latencyService;
        private readonly StringBuilder         _sb = new StringBuilder(1024);

        public LsCommand(LocationService locationService, CommandLatencyService latencyService = null)
        {
            _locationService = locationService;
            _latencyService  = latencyService;
        }

        /// <inheritdoc/>
        public float GetLatency(string[] args)
        {
            if (_latencyService == null) return 0f;
            Device target = null;
            if (args.Length >= 1)
                target = _locationService?.FindDevice(args[0]);
            return _latencyService.CalculateLatency(new CommandLatencyContext("ls", target: target));
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length >= 1)
                return ListFilesOnIp(args[0]);

            return AwaitDeviceSelection();
        }

        // ── Direct IP path ────────────────────────────────────────────────────

        private CommandResult ListFilesOnIp(string ip)
        {
            Device dev = _locationService.FindDevice(ip);
            if (dev == null)
                return CommandResult.Fail($"Device '{ip}' not found at current location.");

            return BuildFileList(dev);
        }

        // ── Interactive selection path ─────────────────────────────────────────

        private CommandResult AwaitDeviceSelection()
        {
            var devices = _locationService.GetScannedDevicesAtCurrentLocation();
            if (devices == null || devices.Count == 0)
                return CommandResult.Fail("No devices discovered at current location. Use 'scan' first.");

            string[] options = new string[devices.Count];
            for (int i = 0; i < devices.Count; i++)
                options[i] = devices[i].Ip;

            UI.TerminalController.Instance?.AwaitSelection(options, idx =>
            {
                if (idx < 0) return;
                var result = BuildFileList(devices[idx]);
                UI.TerminalController.Instance?.AppendOutput(result.Message);
            });

            return CommandResult.Ok("");
        }

        // ── Shared logic ──────────────────────────────────────────────────────

        private CommandResult BuildFileList(Device dev)
        {
            _sb.Clear();
            _sb.AppendLine($"Files on {dev.Ip}:");

            if (dev.Files.Count == 0)
            {
                _sb.AppendLine("  (no files found)");
                return CommandResult.Ok(_sb.ToString());
            }

            for (int i = 0; i < dev.Files.Count; i++)
            {
                DeviceFile f    = dev.Files[i];
                string     size = FormatSize(f.SizeBytes);
                _sb.AppendLine($"  {f.Path,-42} ({size})");
            }

            return CommandResult.Ok(_sb.ToString());
        }

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
            if (bytes >= 1024L * 1024)        return $"{bytes / (1024.0 * 1024):F1} MB";
            if (bytes >= 1024L)               return $"{bytes / 1024.0:F1} KB";
            return $"{bytes} B";
        }
    }
}
