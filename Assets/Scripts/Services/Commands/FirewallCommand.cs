using HackYourWay.Interfaces;
using HackYourWay.Models;
using HackYourWay.Services;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>firewall {disable|enable} [{IP}]</c>.
    /// If IP is provided the device is resolved directly; otherwise interactive device
    /// selection is presented via <see cref="UI.TerminalController.AwaitSelection"/>.
    /// Requires the <see cref="ToolType.FirewallDisable"/> tool.
    /// </summary>
    public class FirewallCommand : ICommand
    {
        private readonly Player                _player;
        private readonly LocationService       _locationService;
        private readonly CommandLatencyService _latencyService;

        public FirewallCommand(Player player, LocationService locationService,
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
            Device target = null;
            if (args.Length >= 2)
                target = _locationService.FindDevice(args[1]);
            return _latencyService.CalculateLatency(new CommandLatencyContext("firewall", target: target));
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("Usage: firewall {disable|enable} [<IP>]");

            if (!_player.HasTool(ToolType.FirewallDisable))
                return CommandResult.Fail("You do not own a firewall tool. Visit the store to purchase one.");

            string sub = args[0].ToLowerInvariant();
            if (sub != "disable" && sub != "enable")
                return CommandResult.Fail($"Unknown action '{args[0]}'. Use 'disable' or 'enable'.");

            if (args.Length >= 2)
                return ApplyToIp(args[1], sub);

            // No IP → interactive device selection
            return AwaitDeviceSelection(sub);
        }

        // ── Direct IP path ────────────────────────────────────────────────────

        private CommandResult ApplyToIp(string ip, string action)
        {
            Device dev = _locationService.FindDevice(ip);
            if (dev == null)
                return CommandResult.Fail($"Device '{ip}' not found at current location.");

            return ApplyAction(dev, action);
        }

        // ── Interactive selection path ─────────────────────────────────────────

        private CommandResult AwaitDeviceSelection(string action)
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
                var result = ApplyAction(devices[idx], action);
                UI.TerminalController.Instance?.AppendOutput(result.Message);
            });

            return CommandResult.Ok("");
        }

        // ── Shared logic ──────────────────────────────────────────────────────

        private static CommandResult ApplyAction(Device dev, string action)
        {
            switch (action)
            {
                case "disable":
                    if (dev.FirewallStatus == FirewallStatus.Disabled)
                        return CommandResult.Fail($"Firewall is already disabled on {dev.Ip}.");
                    dev.FirewallStatus = FirewallStatus.Disabled;
                    return CommandResult.Ok($"Firewall disabled on {dev.Ip}.");

                case "enable":
                    if (dev.FirewallStatus == FirewallStatus.Active)
                        return CommandResult.Fail($"Firewall is already active on {dev.Ip}.");
                    dev.FirewallStatus = FirewallStatus.Active;
                    return CommandResult.Ok($"Firewall re-enabled on {dev.Ip}.");

                default:
                    return CommandResult.Fail($"Unknown action '{action}'.");
            }
        }
    }
}
