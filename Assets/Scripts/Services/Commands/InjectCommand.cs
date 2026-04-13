using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>inject {miner|bot|spammer|ransomware}</c>.
    /// Phase 3 (US1) scope: miner injection only.
    /// Bot, spammer, and ransomware extended in Phase 4 (US2 / T044).
    /// </summary>
    public class InjectCommand : ICommand
    {
        // Base income rate for a freshly installed miner.
        private const double MinerBaseRate = 0.0012;

        private readonly Player _player;

        public InjectCommand(Player player)
        {
            _player = player;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("Usage: inject {miner|bot|spammer|ransomware}");

            // Precondition: device must be targeted.
            if (_player.TargetedDevice == null)
                return CommandResult.Fail("No device targeted. Run 'scan ip {IP}' first.");

            Device  dev = _player.TargetedDevice;
            Network net = _player.TargetedNetwork;

            // Precondition: device must not be already infected.
            if (dev.ActiveMalware != null)
                return CommandResult.Fail($"{dev.Ip} is already infected with a {dev.ActiveMalware.Type.ToString().ToLowerInvariant()}.");

            string type = args[0].ToLowerInvariant();
            switch (type)
            {
                case "miner":      return InjectMiner(dev, net);
                case "bot":        return InjectBot(dev, net);
                case "spammer":    return InjectSpammer(dev, net);
                case "ransomware": return InjectRansomware(dev, net);
                default:
                    return CommandResult.Fail($"Unknown malware type '{args[0]}'. Use miner, bot, spammer, or ransomware.");
            }
        }

        // ── Miner ─────────────────────────────────────────────────────────────

        private CommandResult InjectMiner(Device dev, Network net)
        {
            // Precondition: firewall disabled (unless open network).
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

        // ── Bot (US2 extension — software purchase required) ──────────────────

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

        // ── Spammer ───────────────────────────────────────────────────────────

        private CommandResult InjectSpammer(Device dev, Network net)
        {
            if (!dev.CanInject(net.SecurityLevel))
                return CommandResult.Fail($"Firewall is active on {dev.Ip}. Disable it first.");

            if (!_player.HasTool(ToolType.SpammerSoftware))
                return CommandResult.Fail("You do not own spammer software. Visit the store to purchase it.");

            const double spammerRate = 0.0008;
            dev.ActiveMalware = new Malware
            {
                Type                = MalwareType.Spammer,
                DeviceIp            = dev.Ip,
                IncomeRate          = spammerRate,
                Currency            = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks
            };

            return CommandResult.Ok(
                $"Injecting spammer into {dev.Ip}...\nSpammer installed. Spam contracts are now available.\nGenerating {spammerRate:F4} BTC/s.");
        }

        // ── Ransomware ────────────────────────────────────────────────────────

        private CommandResult InjectRansomware(Device dev, Network net)
        {
            if (!dev.CanInject(net.SecurityLevel))
                return CommandResult.Fail($"Firewall is active on {dev.Ip}. Disable it first.");

            if (!_player.HasTool(ToolType.Ransomware))
                return CommandResult.Fail("You do not own ransomware software. Visit the store to purchase it.");

            // RansomAmount is set at device generation time via LocationConfigSO.
            // For runtime injection the default is 0.05 BTC; designers override via StoreItemSO.
            const double defaultRansom = 0.05;
            dev.ActiveMalware = new Malware
            {
                Type                = MalwareType.Ransomware,
                DeviceIp            = dev.Ip,
                IncomeRate          = 0.0, // ransomware has no per-tick income
                Currency            = CurrencyType.Bitcoin,
                InstalledAtUtcTicks = System.DateTime.UtcNow.Ticks,
                RansomAmount        = defaultRansom
            };

            return CommandResult.Ok(
                $"Injecting ransomware into {dev.Ip}...\nDevice encrypted. Ransom demand: {defaultRansom:F4} BTC.\nAwaiting payment...");
        }
    }
}
