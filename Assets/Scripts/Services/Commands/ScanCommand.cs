using System.Text;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements the <c>scan</c> command family.
    /// <list type="bullet">
    ///   <item><c>scan</c> — lists networks at current location. Does not change location.</item>
    ///   <item><c>scan network {SSID}</c> — lists devices on a known network.</item>
    ///   <item><c>scan ip {IP}</c> — shows firewall/ports for a device on an accessible network.</item>
    ///   <item><c>scan mac {MAC}</c> — same as scan ip but by MAC address.</item>
    /// </list>
    /// </summary>
    public class ScanCommand : ICommand
    {
        private readonly Player          _player;
        private readonly LocationService _locationService;

        // Reused builder — avoids per-call allocation (Constitution Principle IV).
        private readonly StringBuilder _sb = new StringBuilder(512);

        public ScanCommand(Player player, LocationService locationService)
        {
            _player          = player;
            _locationService = locationService;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length == 0)
                return ScanArea();

            if (args.Length >= 2)
            {
                string sub = args[0].ToLowerInvariant();
                if (sub == "network") return ScanNetwork(args[1]);
                if (sub == "ip")      return ScanDevice(args[1], byMac: false);
                if (sub == "mac")     return ScanDevice(args[1], byMac: true);
            }

            return CommandResult.Fail($"Usage: scan | scan network {{SSID}} | scan ip {{IP}} | scan mac {{MAC}}");
        }

        // ── scan (no args) ────────────────────────────────────────────────────

        private CommandResult ScanArea()
        {
            if (!_locationService.HasCurrentLocation())
                return CommandResult.Fail("No location available. Use 'move' to discover a location first.");

            Location loc = _locationService.GetCurrentLocation();
            _sb.Clear();
            AppendNetworkList(loc);
            return CommandResult.Ok(_sb.ToString());
        }

        private void AppendNetworkList(Location loc)
        {
            _sb.AppendLine("Scanning area...");
            _sb.AppendLine($"Found {loc.Networks.Count} network(s):");
            _sb.AppendLine();

            for (int i = 0; i < loc.Networks.Count; i++)
            {
                var net = loc.Networks[i];
                string label = net.SecurityLevel == SecurityLevel.None ? "OPEN" : net.SecurityLevel.ToString();
                _sb.AppendLine($"  [{i + 1}] {net.Ssid,-24} [{label}]");
            }
        }

        // ── scan network {SSID} ────────────────────────────────────────────────

        private CommandResult ScanNetwork(string ssid)
        {
            Location loc = _locationService.GetCurrentLocation();
            Network  net = FindNetworkBySsid(loc, ssid);

            if (net == null)
                return CommandResult.Fail($"Network '{ssid}' not found. Run 'scan' first.");

            net.IsScanned = true;

            _sb.Clear();
            _sb.AppendLine($"Scanning {ssid}...");
            _sb.AppendLine($"Security: {net.SecurityLevel}");
            _sb.AppendLine($"Devices found: {net.Devices.Count}");
            _sb.AppendLine();

            for (int i = 0; i < net.Devices.Count; i++)
                _sb.AppendLine($"  {net.Devices[i].Ip}");

            return CommandResult.Ok(_sb.ToString());
        }

        // ── scan ip / scan mac ────────────────────────────────────────────────

        private CommandResult ScanDevice(string identifier, bool byMac)
        {
            Location loc = _locationService.GetCurrentLocation();
            Network  foundNet    = null;
            Device   foundDevice = null;

            for (int n = 0; n < loc.Networks.Count; n++)
            {
                var net = loc.Networks[n];
                if (!net.IsHacked && net.SecurityLevel != SecurityLevel.None)
                    continue; // no access

                for (int d = 0; d < net.Devices.Count; d++)
                {
                    var dev = net.Devices[d];
                    bool match = byMac
                        ? dev.Mac  == identifier
                        : dev.Ip   == identifier;

                    if (match)
                    {
                        foundNet    = net;
                        foundDevice = dev;
                        break;
                    }
                }

                if (foundDevice != null) break;
            }

            if (foundDevice == null)
            {
                string label = byMac ? "MAC address" : "IP";
                return CommandResult.Fail($"{label} '{identifier}' not found on any accessible network.");
            }

            foundDevice.IsScanned = true;

            _sb.Clear();
            _sb.AppendLine($"Scanning {foundDevice.Ip}...");
            _sb.AppendLine($"Firewall: {(foundDevice.FirewallStatus == FirewallStatus.Active ? "ACTIVE" : "DISABLED")}");

            _sb.Append("Open ports: ");
            for (int i = 0; i < foundDevice.OpenPorts.Count; i++)
            {
                if (i > 0) _sb.Append(", ");
                _sb.Append(foundDevice.OpenPorts[i]);
            }
            _sb.AppendLine();

            if (foundDevice.ActiveMalware != null)
            {
                var m = foundDevice.ActiveMalware;
                _sb.AppendLine($"Malware: {m.Type.ToString().ToLowerInvariant()} [{m.Currency}] — {m.IncomeRate:F4} {m.Currency}/s");
            }

            return CommandResult.Ok(_sb.ToString());
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static Network FindNetworkBySsid(Location loc, string ssid)
        {
            for (int i = 0; i < loc.Networks.Count; i++)
                if (loc.Networks[i].Ssid == ssid) return loc.Networks[i];
            return null;
        }
    }
}
