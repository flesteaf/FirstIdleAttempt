using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>forget network {SSID} [at {location}]</c> and <c>forget ip {IP}</c>.
    /// Removes the specified network (and all its devices) or a single device from all location records,
    /// stopping their income contributions immediately.
    /// When a network SSID is ambiguous (exists at multiple locations), interactive selection is shown.
    /// </summary>
    public class ForgetCommand : ICommand
    {
        private readonly LocationService _locationService;

        public ForgetCommand(LocationService locationService)
        {
            _locationService = locationService;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length < 2)
                return UsageError();

            string sub = args[0].ToLowerInvariant();

            switch (sub)
            {
                case "network":
                {
                    string ssid         = args[1];
                    string locationName = null;

                    // forget network {SSID} at {location}
                    if (args.Length >= 4 && args[2].ToLowerInvariant() == "at")
                        locationName = args[3];

                    return ForgetNetwork(ssid, locationName);
                }

                case "ip":
                    return ForgetIp(args[1]);

                default:
                    return UsageError();
            }
        }

        private CommandResult ForgetNetwork(string ssid, string locationName)
        {
            ForgetResult result = _locationService.ForgetNetwork(ssid, locationName);

            if (!result.Success && result.Message.StartsWith("Network found at multiple locations:"))
            {
                // Extract location names from the Ambiguous message and offer selection
                string[] parts = result.Message.Split(':');
                string locationList = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                string[] locationNames = locationList.Split(' ');

                UI.TerminalController.Instance?.AwaitSelection(locationNames, idx =>
                {
                    if (idx < 0) return;
                    ForgetResult scoped = _locationService.ForgetNetwork(ssid, locationNames[idx]);
                    UI.TerminalController.Instance?.AppendOutput(scoped.Message);
                });

                return CommandResult.Ok("");
            }

            return result.Success
                ? CommandResult.Ok(result.Message)
                : CommandResult.Fail(result.Message);
        }

        private CommandResult ForgetIp(string ip)
        {
            ForgetResult result = _locationService.ForgetDevice(ip);
            return result.Success
                ? CommandResult.Ok(result.Message)
                : CommandResult.Fail(result.Message);
        }

        private static CommandResult UsageError() =>
            CommandResult.Fail(
                "Usage: forget network {SSID} | forget network {SSID} at {location} | forget ip {IP}");
    }
}
