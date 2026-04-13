using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>firewall {disable|enable}</c>.
    /// Requires the <see cref="ToolType.FirewallDisable"/> tool and a currently targeted device.
    /// </summary>
    public class FirewallCommand : ICommand
    {
        private readonly Player _player;

        public FirewallCommand(Player player)
        {
            _player = player;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("Usage: firewall {disable|enable}");

            // Precondition: player must own the firewall tool.
            if (!_player.HasTool(ToolType.FirewallDisable))
                return CommandResult.Fail("You do not own a firewall tool. Visit the store to purchase one.");

            // Precondition: a device must be targeted.
            if (_player.TargetedDevice == null)
                return CommandResult.Fail("No device targeted. Run 'scan ip {IP}' first.");

            Device dev = _player.TargetedDevice;
            string sub = args[0].ToLowerInvariant();

            switch (sub)
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
                    return CommandResult.Fail($"Unknown action '{args[0]}'. Use 'disable' or 'enable'.");
            }
        }
    }
}
