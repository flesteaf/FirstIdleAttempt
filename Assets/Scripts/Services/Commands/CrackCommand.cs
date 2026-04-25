using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>crack {WEP|WPA|WPA2} {SSID}</c>.
    /// Validates tool ownership, security level match, and that the network is not already hacked.
    /// </summary>
    public class CrackCommand : ICommand
    {
        private readonly Player                 _player;
        private readonly LocationService        _locationService;
        private readonly CommandLatencyService  _latencyService;

        public CrackCommand(Player player, LocationService locationService,
                            CommandLatencyService latencyService = null)
        {
            _player          = player;
            _locationService = locationService;
            _latencyService  = latencyService;
        }

        /// <inheritdoc/>
        public float GetLatency(string[] args)
        {
            if (_latencyService == null || args.Length < 1) return 0f;
            return TryParseSecurityLevel(args[0].ToUpperInvariant(), out SecurityLevel level)
                ? _latencyService.CalculateLatency(new CommandLatencyContext("crack", secLevel: level))
                : 0f;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length < 2)
                return CommandResult.Fail("Usage: crack {WEP|WPA|WPA2} {SSID}");

            string typeArg = args[0].ToUpperInvariant();
            string ssid    = args[1];

            // Resolve declared security level.
            if (!TryParseSecurityLevel(typeArg, out SecurityLevel declaredLevel))
                return CommandResult.Fail($"Unknown security type '{typeArg}'. Use WEP, WPA, or WPA2.");

            // Find network in current location.
            Location loc = _locationService.GetCurrentLocation();
            Network  net = FindBySsid(loc, ssid);
            if (net == null)
                return CommandResult.Fail($"Network '{ssid}' not found. Run 'scan' first.");

            // Already hacked?
            if (net.IsHacked)
                return CommandResult.Fail($"Network '{ssid}' is already hacked.");

            // Type arg must match actual security level.
            if (net.SecurityLevel != declaredLevel)
                return CommandResult.Fail(
                    $"Network '{ssid}' uses {net.SecurityLevel} security, not {declaredLevel}.");

            // Player must own the corresponding tool.
            ToolType required = SecurityLevelToTool(declaredLevel);
            if (!_player.HasTool(required))
                return CommandResult.Fail(
                    $"You do not own a {declaredLevel} cracking tool. Visit the store to purchase one.");

            // Grant access.
            net.IsHacked = true;
            return CommandResult.Ok($"Cracking {ssid} [{declaredLevel}]...\nAccess granted. Network is now under your control.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool TryParseSecurityLevel(string arg, out SecurityLevel level)
        {
            switch (arg)
            {
                case "WEP":  level = SecurityLevel.WEP;  return true;
                case "WPA":  level = SecurityLevel.WPA;  return true;
                case "WPA2": level = SecurityLevel.WPA2; return true;
                default:     level = default;             return false;
            }
        }

        private static ToolType SecurityLevelToTool(SecurityLevel level)
        {
            switch (level)
            {
                case SecurityLevel.WEP:  return ToolType.CrackWEP;
                case SecurityLevel.WPA:  return ToolType.CrackWPA;
                case SecurityLevel.WPA2: return ToolType.CrackWPA2;
                default:                 return ToolType.CrackWEP;
            }
        }

        private static Network FindBySsid(Location loc, string ssid)
        {
            for (int i = 0; i < loc.Networks.Count; i++)
                if (loc.Networks[i].Ssid == ssid) return loc.Networks[i];
            return null;
        }
    }
}
