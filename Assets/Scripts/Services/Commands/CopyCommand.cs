using HackYourWay.Interfaces;
using HackYourWay.Models;
using HackYourWay.Services;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>copy {filename} [{IP}]</c> — copies a file from a device to
    /// <see cref="Player.CopiedFiles"/>. If IP is omitted, interactive device selection
    /// is presented via <see cref="UI.TerminalController.AwaitSelection"/>.
    /// </summary>
    public class CopyCommand : ICommand
    {
        private readonly Player                _player;
        private readonly LocationService       _locationService;
        private readonly CommandLatencyService _latencyService;

        public CopyCommand(Player player, LocationService locationService,
                           CommandLatencyService latencyService = null)
        {
            _player          = player;
            _locationService = locationService;
            _latencyService  = latencyService;
        }

        /// <inheritdoc/>
        public float GetLatency(string[] args)
        {
            if (_latencyService == null) return 0f;
            Device target   = null;
            long   fileSize = 0;
            if (args.Length >= 2)
            {
                target = _locationService.FindDevice(args[1]);
                if (target != null)
                {
                    string query = args[0];
                    for (int i = 0; i < target.Files.Count; i++)
                    {
                        var f = target.Files[i];
                        if (f.Path == query || f.Name == query) { fileSize = f.SizeBytes; break; }
                    }
                }
            }
            return _latencyService.CalculateLatency(
                new CommandLatencyContext("copy", target: target, fileSize: fileSize));
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("Usage: copy {filename} [<IP>]");

            string filename = args[0];

            if (args.Length >= 2)
                return CopyFromIp(filename, args[1]);

            return AwaitDeviceSelection(filename);
        }

        // ── Direct IP path ────────────────────────────────────────────────────

        private CommandResult CopyFromIp(string filename, string ip)
        {
            Device dev = _locationService.FindDevice(ip);
            if (dev == null)
                return CommandResult.Fail($"Device '{ip}' not found at current location.");

            return CopyFile(filename, dev);
        }

        // ── Interactive selection path ─────────────────────────────────────────

        private CommandResult AwaitDeviceSelection(string filename)
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
                var result = CopyFile(filename, devices[idx]);
                UI.TerminalController.Instance?.AppendOutput(result.Message);
            });

            return CommandResult.Ok("");
        }

        // ── Shared logic ──────────────────────────────────────────────────────

        private CommandResult CopyFile(string query, Device dev)
        {
            DeviceFile found = null;
            for (int i = 0; i < dev.Files.Count; i++)
            {
                var f = dev.Files[i];
                if (f.Path == query || f.Name == query)
                {
                    found = f;
                    break;
                }
            }

            if (found == null)
                return CommandResult.Fail($"File '{query}' not found on {dev.Ip}.");

            if (!_player.CopiedFiles.Contains(found.Path))
                _player.CopiedFiles.Add(found.Path);

            return CommandResult.Ok($"Copying {found.Path}...\nFile copied to your storage.");
        }
    }
}
