using System.Collections.Generic;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements the <c>inject</c> command family.
    /// <list type="bullet">
    ///   <item><c>inject {type} {IP} {SSID}</c> — injects directly, no prompts.</item>
    ///   <item><c>inject {type}</c> — prompts for network selection.</item>
    ///   <item><c>inject</c> — prompts for type then network selection.</item>
    /// </list>
    /// </summary>
    public class InjectCommand : ICommand
    {
        private const double MinerBaseRate   = 0.0012;
        private const double SpammerBaseRate = 0.0008;
        private const double DefaultRansomBtc = 0.05;

        private readonly Player          _player;
        private readonly LocationService _locationService;

        public InjectCommand(Player player, LocationService locationService = null)
        {
            _player          = player;
            _locationService = locationService;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            // Full direct path: inject {type} {IP} {SSID}
            if (args.Length >= 3)
            {
                if (!TryResolveDevice(args[1], args[2], out Device dev, out Network net, out string err))
                    return CommandResult.Fail(err);
                return DispatchInject(args[0], dev, net);
            }

            // Partial direct path: inject {type} {IP_only} — ambiguous, require SSID too
            if (args.Length == 2)
                return CommandResult.Fail("Usage: inject {type} {IP} {SSID}");

            // Interactive: inject {type} — skip type selection, go to network selection
            if (args.Length == 1)
                return BeginNetworkSelection(args[0]);

            // Interactive: inject — begin type selection first
            return BeginTypeSelection();
        }

        // ── Interactive type selection (inject with no args) ──────────────────

        private CommandResult BeginTypeSelection()
        {
            var types = GetAvailableTypes();
            if (types.Count == 0)
                return CommandResult.Fail("No malware types available.");

            string[] options = types.ToArray();
            UI.TerminalController.Instance?.AwaitSelection(options, idx =>
            {
                if (idx < 0) return;
                string chosen = options[idx];
                var result = BeginNetworkSelection(chosen);
                // If no interactive was started (e.g. error), show the message
                if (!string.IsNullOrEmpty(result.Message))
                    UI.TerminalController.Instance?.AppendOutput(result.Message);
            });
            return CommandResult.Ok("");
        }

        // ── Interactive network selection ─────────────────────────────────────

        private CommandResult BeginNetworkSelection(string typeName)
        {
            if (_locationService == null)
                return CommandResult.Fail("No location service available.");

            if (!_locationService.HasCurrentLocation())
                return CommandResult.Fail("No location available. Use 'move' to discover a location first.");

            Location loc = _locationService.GetCurrentLocation();
            var accessible = new List<Network>();
            for (int n = 0; n < loc.Networks.Count; n++)
            {
                var net = loc.Networks[n];
                if (net.IsHacked || net.SecurityLevel == SecurityLevel.None)
                    accessible.Add(net);
            }

            if (accessible.Count == 0)
                return CommandResult.Fail("No accessible networks at current location. Crack a network first.");

            string[] options = new string[accessible.Count];
            for (int i = 0; i < accessible.Count; i++)
                options[i] = accessible[i].Ssid;

            UI.TerminalController.Instance?.AwaitSelection(options, idx =>
            {
                if (idx < 0) return;
                Network chosen = accessible[idx];
                Device  dev    = FindInjectableDevice(chosen);
                if (dev == null)
                {
                    UI.TerminalController.Instance?.AppendOutput(
                        $"Error: No injectable device found on '{chosen.Ssid}'.");
                    return;
                }
                var result = DispatchInject(typeName, dev, chosen);
                UI.TerminalController.Instance?.AppendOutput(result.Message);
            });

            return CommandResult.Ok("");
        }

        // ── Dispatch to specific malware injectors ────────────────────────────

        private CommandResult DispatchInject(string typeName, Device dev, Network net)
        {
            if (dev.ActiveMalware != null)
                return CommandResult.Fail(
                    $"{dev.Ip} is already infected with a {dev.ActiveMalware.Type.ToString().ToLowerInvariant()}.");

            switch (typeName.ToLowerInvariant())
            {
                case "miner":      return InjectMiner(dev, net);
                case "bot":        return InjectBot(dev, net);
                case "spammer":    return InjectSpammer(dev, net);
                case "ransomware": return InjectRansomware(dev, net);
                default:
                    return CommandResult.Fail(
                        $"Unknown malware type '{typeName}'. Use miner, bot, spammer, or ransomware.");
            }
        }

        // ── Device resolution ─────────────────────────────────────────────────

        private bool TryResolveDevice(
            string ip, string ssid,
            out Device device, out Network network, out string error)
        {
            device  = null;
            network = null;
            error   = null;

            if (_locationService == null)
            {
                error = "No location service available.";
                return false;
            }

            Location loc = _locationService.GetCurrentLocation();

            Network found = null;
            for (int n = 0; n < loc.Networks.Count; n++)
            {
                if (loc.Networks[n].Ssid == ssid) { found = loc.Networks[n]; break; }
            }

            if (found == null)
            {
                error = $"Network '{ssid}' not found at current location. Run 'scan' first.";
                return false;
            }

            if (!found.IsHacked && found.SecurityLevel != SecurityLevel.None)
            {
                error = $"Network '{ssid}' is not accessible. Crack it first or target an open network.";
                return false;
            }

            Device foundDevice = null;
            for (int d = 0; d < found.Devices.Count; d++)
            {
                if (found.Devices[d].Ip == ip) { foundDevice = found.Devices[d]; break; }
            }

            if (foundDevice == null)
            {
                error = $"Device '{ip}' not found on network '{ssid}'.";
                return false;
            }

            device  = foundDevice;
            network = found;
            return true;
        }

        private static Device FindInjectableDevice(Network net)
        {
            for (int d = 0; d < net.Devices.Count; d++)
            {
                var dev = net.Devices[d];
                if (dev.ActiveMalware == null && dev.CanInject(net.SecurityLevel))
                    return dev;
            }
            return null;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private List<string> GetAvailableTypes()
        {
            var types = new List<string> { "miner" };
            if (_player.HasTool(ToolType.BotSoftware))      types.Add("bot");
            if (_player.HasTool(ToolType.SpammerSoftware))  types.Add("spammer");
            if (_player.HasTool(ToolType.Ransomware))       types.Add("ransomware");
            return types;
        }

        // ── Malware injectors ─────────────────────────────────────────────────

        private CommandResult InjectMiner(Device dev, Network net)
        {
            if (!dev.CanInject(net.SecurityLevel))
                return CommandResult.Fail($"Firewall is active on {dev.Ip}. Disable it first.");

            dev.ActiveMalware = new Malware
            {
                Type                = MalwareType.Miner,
                DeviceIp            = dev.Ip,
                IncomeRate          = MinerBaseRate,
                Currency            = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks
            };
            return CommandResult.Ok($"Injecting miner into {dev.Ip}...\nMiner installed. Generating {MinerBaseRate:F4} BTC/s.");
        }

        private CommandResult InjectBot(Device dev, Network net)
        {
            if (!dev.CanInject(net.SecurityLevel))
                return CommandResult.Fail($"Firewall is active on {dev.Ip}. Disable it first.");

            if (!_player.HasTool(ToolType.BotSoftware))
                return CommandResult.Fail("You do not own bot injection software. Visit the store to purchase it.");

            dev.ActiveMalware = new Malware
            {
                Type                = MalwareType.Bot,
                DeviceIp            = dev.Ip,
                IncomeRate          = 0.0,
                Currency            = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks
            };
            return CommandResult.Ok($"Injecting bot into {dev.Ip}...\nBot installed. Attack contracts are now available.");
        }

        private CommandResult InjectSpammer(Device dev, Network net)
        {
            if (!dev.CanInject(net.SecurityLevel))
                return CommandResult.Fail($"Firewall is active on {dev.Ip}. Disable it first.");

            if (!_player.HasTool(ToolType.SpammerSoftware))
                return CommandResult.Fail("You do not own spammer software. Visit the store to purchase it.");

            dev.ActiveMalware = new Malware
            {
                Type                = MalwareType.Spammer,
                DeviceIp            = dev.Ip,
                IncomeRate          = SpammerBaseRate,
                Currency            = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks
            };
            return CommandResult.Ok(
                $"Injecting spammer into {dev.Ip}...\nSpammer installed. Spam contracts are now available.\nGenerating {SpammerBaseRate:F4} BTC/s.");
        }

        private CommandResult InjectRansomware(Device dev, Network net)
        {
            if (!dev.CanInject(net.SecurityLevel))
                return CommandResult.Fail($"Firewall is active on {dev.Ip}. Disable it first.");

            if (!_player.HasTool(ToolType.Ransomware))
                return CommandResult.Fail("You do not own ransomware software. Visit the store to purchase it.");

            double ransom = DefaultRansomBtc;
            if (_locationService != null)
            {
                var cfg = _locationService.Config;
                ransom = (cfg.MinRansomAmount + cfg.MaxRansomAmount) * 0.5;
            }

            dev.ActiveMalware = new Malware
            {
                Type                = MalwareType.Ransomware,
                DeviceIp            = dev.Ip,
                IncomeRate          = 0.0,
                Currency            = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks,
                RansomAmount        = ransom
            };
            return CommandResult.Ok(
                $"Injecting ransomware into {dev.Ip}...\nDevice encrypted. Ransom demand: {ransom:F4} BTC.\nAwaiting payment...");
        }
    }
}
