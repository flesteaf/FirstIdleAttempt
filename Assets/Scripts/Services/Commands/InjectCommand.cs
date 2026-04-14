using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements the <c>inject</c> command family.
    /// <list type="bullet">
    ///   <item><c>inject {type}</c> — injects into the currently targeted device
    ///   (set via <c>scan ip</c> or <c>scan mac</c>). Schema v1.0.0.</item>
    ///   <item><c>inject {type} {IP} {SSID}</c> — resolves target device directly from
    ///   the current location without requiring a prior <c>scan ip</c>. Schema v1.1.0.</item>
    /// </list>
    /// All four malware types support both forms. IP and SSID are optional but
    /// must both be present if either is given.
    /// </summary>
    public class InjectCommand : ICommand
    {
        // Base income rates for freshly installed malware.
        private const double MinerBaseRate    = 0.0012;
        private const double SpammerBaseRate  = 0.0008;

        // Fallback ransom when no LocationService is wired (e.g. unit tests).
        private const double DefaultRansomBtc = 0.05;

        private readonly Player          _player;
        /// <param name="_locationService">Used for direct-inject device resolution and
        /// designer-configured ransom amounts. Nullable — legacy path works without it.</param>
        private readonly LocationService _locationService;

        /// <summary>
        /// Initialises the inject command.
        /// </summary>
        /// <param name="player">Active player session — provides tool ownership and
        /// session targeting (<see cref="Player.TargetedDevice"/>).</param>
        /// <param name="locationService">Used to resolve devices by IP+SSID when the
        /// optional direct-target arguments are supplied, and to read designer-configured
        /// ransom amounts. Pass <c>null</c> only in test contexts that do not exercise
        /// these paths.</param>
        public InjectCommand(Player player, LocationService locationService = null)
        {
            _player          = player;
            _locationService = locationService;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("Usage: inject {miner|bot|spammer|ransomware} [{IP} {SSID}]");

            // Exactly one trailing arg is ambiguous — require both IP and SSID or neither.
            if (args.Length == 2)
                return CommandResult.Fail("Usage: inject {miner|bot|spammer|ransomware} [{IP} {SSID}]");

            Device  dev;
            Network net;

            if (args.Length >= 3)
            {
                // Direct-resolve path (Schema v1.1.0): args[1] = IP, args[2] = SSID.
                if (!TryResolveDevice(args[1], args[2], out dev, out net, out string resolveError))
                    return CommandResult.Fail(resolveError);
            }
            else
            {
                // Legacy path (Schema v1.0.0): use Player.TargetedDevice.
                if (_player.TargetedDevice == null)
                    return CommandResult.Fail("No device targeted. Run 'scan ip {IP}' first.");

                dev = _player.TargetedDevice;
                net = _player.TargetedNetwork;
            }

            // Precondition: device must not already be infected (applies to both paths).
            if (dev.ActiveMalware != null)
                return CommandResult.Fail(
                    $"{dev.Ip} is already infected with a {dev.ActiveMalware.Type.ToString().ToLowerInvariant()}.");

            string type = args[0].ToLowerInvariant();
            switch (type)
            {
                case "miner":      return InjectMiner(dev, net);
                case "bot":        return InjectBot(dev, net);
                case "spammer":    return InjectSpammer(dev, net);
                case "ransomware": return InjectRansomware(dev, net);
                default:
                    return CommandResult.Fail(
                        $"Unknown malware type '{args[0]}'. Use miner, bot, spammer, or ransomware.");
            }
        }

        // ── Device resolution (direct-inject path) ────────────────────────────

        /// <summary>
        /// Attempts to resolve a <see cref="Device"/> and its parent <see cref="Network"/>
        /// by IP and SSID from the current location.
        /// </summary>
        /// <param name="ip">IP address to match.</param>
        /// <param name="ssid">SSID of the containing network.</param>
        /// <param name="device">Resolved device; <c>null</c> on failure.</param>
        /// <param name="network">Resolved network; <c>null</c> on failure.</param>
        /// <param name="error">Human-readable error on failure (without "Error:" prefix);
        /// <c>null</c> on success.</param>
        /// <returns><c>true</c> when both device and network are resolved.</returns>
        private bool TryResolveDevice(
            string ip, string ssid,
            out Device device, out Network network, out string error)
        {
            device  = null;
            network = null;
            error   = null;

            Location loc = _locationService.GetCurrentLocation();

            // Find the network by SSID — for loop, no LINQ (Constitution Principle IV).
            Network found = null;
            for (int n = 0; n < loc.Networks.Count; n++)
            {
                if (loc.Networks[n].Ssid == ssid)
                {
                    found = loc.Networks[n];
                    break;
                }
            }

            if (found == null)
            {
                error = $"Network '{ssid}' not found at current location. Run 'scan' first.";
                return false;
            }

            // Network must be accessible: cracked or open.
            if (!found.IsHacked && found.SecurityLevel != SecurityLevel.None)
            {
                error = $"Network '{ssid}' is not accessible. Crack it first or target an open network.";
                return false;
            }

            // Find the device by IP — for loop, no LINQ.
            Device foundDevice = null;
            for (int d = 0; d < found.Devices.Count; d++)
            {
                if (found.Devices[d].Ip == ip)
                {
                    foundDevice = found.Devices[d];
                    break;
                }
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

        // ── Miner ─────────────────────────────────────────────────────────────

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

            return CommandResult.Ok(
                $"Injecting miner into {dev.Ip}...\nMiner installed. Generating {MinerBaseRate:F4} BTC/s.");
        }

        // ── Bot ───────────────────────────────────────────────────────────────

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

            return CommandResult.Ok(
                $"Injecting bot into {dev.Ip}...\nBot installed. Attack contracts are now available.");
        }

        // ── Spammer ───────────────────────────────────────────────────────────

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

        // ── Ransomware ────────────────────────────────────────────────────────

        private CommandResult InjectRansomware(Device dev, Network net)
        {
            if (!dev.CanInject(net.SecurityLevel))
                return CommandResult.Fail($"Firewall is active on {dev.Ip}. Disable it first.");

            if (!_player.HasTool(ToolType.Ransomware))
                return CommandResult.Fail("You do not own ransomware software. Visit the store to purchase it.");

            // Use the designer-configured ransom range midpoint when available;
            // fall back to the built-in constant when no LocationService is wired (e.g. tests).
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
