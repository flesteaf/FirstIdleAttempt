using System.Text;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>show networks</c>, <c>show ips</c>, and <c>show locations</c>.
    /// All three sub-commands aggregate across ALL known locations and always render column headers.
    /// </summary>
    public class ShowCommand : ICommand
    {
        private readonly LocationService _locationService;
        private readonly StringBuilder   _sb = new StringBuilder(512);

        public ShowCommand(LocationService locationService)
        {
            _locationService = locationService;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("Usage: show {networks|ips|locations}");

            switch (args[0].ToLowerInvariant())
            {
                case "networks":  return ShowNetworks();
                case "ips":       return ShowIps();
                case "locations": return ShowLocations();
                default:
                    return CommandResult.Fail($"Unknown option '{args[0]}'. Use 'networks', 'ips', or 'locations'.");
            }
        }

        // ── show networks ─────────────────────────────────────────────────────

        private CommandResult ShowNetworks()
        {
            var locations = _locationService.GetAllKnownLocations();

            _sb.Clear();
            _sb.AppendLine("NETWORK                   LOCATION     SECURITY  STATUS");

            for (int l = 0; l < locations.Count; l++)
            {
                var loc = locations[l];
                for (int n = 0; n < loc.Networks.Count; n++)
                {
                    var net    = loc.Networks[n];
                    string sec = net.SecurityLevel == SecurityLevel.None ? "OPEN" : net.SecurityLevel.ToString();
                    string status = net.IsHacked
                        ? $"HACKED ({CountInfected(net)} infected)"
                        : "not cracked";

                    _sb.AppendLine($"  {net.Ssid,-24} {loc.Name,-12} {sec,-9} {status}");
                }
            }

            return CommandResult.Ok(_sb.ToString());
        }

        // ── show ips ──────────────────────────────────────────────────────────

        private CommandResult ShowIps()
        {
            var locations = _locationService.GetAllKnownLocations();

            _sb.Clear();
            _sb.AppendLine("IP                 NETWORK                   LOCATION     TYPE         INCOME/s");

            for (int l = 0; l < locations.Count; l++)
            {
                var loc = locations[l];
                for (int n = 0; n < loc.Networks.Count; n++)
                {
                    var net = loc.Networks[n];
                    for (int d = 0; d < net.Devices.Count; d++)
                    {
                        var dev = net.Devices[d];
                        if (dev.ActiveMalware == null) continue;

                        var m      = dev.ActiveMalware;
                        string inc = m.IncomeRate > 0 ? $"{m.IncomeRate:F4} {m.Currency}/s" : "(no income)";

                        _sb.AppendLine(
                            $"  {dev.Ip,-18} {net.Ssid,-24} {loc.Name,-12} {m.Type.ToString().ToLowerInvariant(),-12} {inc}");
                    }
                }
            }

            return CommandResult.Ok(_sb.ToString());
        }

        // ── show locations ────────────────────────────────────────────────────

        private CommandResult ShowLocations()
        {
            var locations    = _locationService.GetAllKnownLocations();
            string currentName = _locationService.GetCurrentLocationName();

            _sb.Clear();
            _sb.AppendLine("LOCATION      NETWORKS  INFECTED");

            for (int l = 0; l < locations.Count; l++)
            {
                var loc      = locations[l];
                string marker = loc.Name == currentName ? ">" : " ";
                int networks  = loc.Networks.Count;
                int infected  = 0;

                for (int n = 0; n < loc.Networks.Count; n++)
                    infected += CountInfected(loc.Networks[n]);

                _sb.AppendLine($"  {marker} {loc.Name,-12}  {networks,-9} {infected}");
            }

            return CommandResult.Ok(_sb.ToString());
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static int CountInfected(Network net)
        {
            int count = 0;
            for (int i = 0; i < net.Devices.Count; i++)
                if (net.Devices[i].ActiveMalware != null) count++;
            return count;
        }
    }
}
