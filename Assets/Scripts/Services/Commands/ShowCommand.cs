using System.Text;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>show networks</c> and <c>show ips</c>.
    /// Displays known network statuses and all infected devices.
    /// </summary>
    public class ShowCommand : ICommand
    {
        private readonly Player          _player;
        private readonly LocationService _locationService;

        private readonly StringBuilder _sb = new StringBuilder(512);

        public ShowCommand(Player player, LocationService locationService)
        {
            _player          = player;
            _locationService = locationService;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("Usage: show {networks|ips}");

            string sub = args[0].ToLowerInvariant();
            switch (sub)
            {
                case "networks": return ShowNetworks();
                case "ips":      return ShowIps();
                default:
                    return CommandResult.Fail($"Unknown option '{args[0]}'. Use 'networks' or 'ips'.");
            }
        }

        // ── show networks ─────────────────────────────────────────────────────

        private CommandResult ShowNetworks()
        {
            Location loc = _locationService.GetCurrentLocation();

            _sb.Clear();
            _sb.AppendLine("Known networks:");
            _sb.AppendLine();

            for (int n = 0; n < loc.Networks.Count; n++)
            {
                var net = loc.Networks[n];
                int infected = CountInfected(net);

                if (net.IsHacked)
                {
                    _sb.AppendLine(
                        $"  [HACKED]  {net.Ssid,-24} 192.168.x.x ({net.Devices.Count} devices, {infected} infected)");
                }
                else
                {
                    string label = net.SecurityLevel == SecurityLevel.None ? "OPEN" : net.SecurityLevel.ToString();
                    string status = net.SecurityLevel == SecurityLevel.None ? "— not infiltrated" : "— not cracked";
                    _sb.AppendLine($"  [FOUND]   {net.Ssid,-24} [{label}] {status}");
                }
            }

            return CommandResult.Ok(_sb.ToString());
        }

        private static int CountInfected(Network net)
        {
            int count = 0;
            for (int i = 0; i < net.Devices.Count; i++)
                if (net.Devices[i].ActiveMalware != null) count++;
            return count;
        }

        // ── show ips ──────────────────────────────────────────────────────────

        private CommandResult ShowIps()
        {
            Location loc = _locationService.GetCurrentLocation();

            _sb.Clear();

            bool any = false;
            for (int n = 0; n < loc.Networks.Count; n++)
            {
                var net = loc.Networks[n];
                for (int d = 0; d < net.Devices.Count; d++)
                {
                    var dev = net.Devices[d];
                    if (dev.ActiveMalware == null) continue;

                    if (!any)
                    {
                        _sb.AppendLine("Infected devices:");
                        _sb.AppendLine();
                        any = true;
                    }

                    var m = dev.ActiveMalware;
                    string income = m.IncomeRate > 0
                        ? $"{m.IncomeRate:F4} {m.Currency}/s"
                        : "(no income)";

                    _sb.AppendLine($"  {dev.Ip,-18} {m.Type.ToString().ToLowerInvariant(),-12} {income}");
                }
            }

            if (!any)
                _sb.AppendLine("No infected devices yet. Hack a network and inject malware to get started.");

            return CommandResult.Ok(_sb.ToString());
        }
    }
}
