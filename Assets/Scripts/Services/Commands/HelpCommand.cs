using System.Collections.Generic;
using System.Text;
using HackYourWay.Interfaces;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements the <c>help</c> command.
    /// With no arguments: lists all registered command verbs with a one-line description.
    /// With a verb argument (e.g. <c>help scan</c>): shows detailed usage for that command.
    /// </summary>
    public class HelpCommand : ICommand
    {
        private static readonly Dictionary<string, string> s_usage =
            new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "help",     "help [command]                            — show this list or detail for a command" },
                { "scan",     "scan                                      — discover networks; scan ip/mac shows CPU+BW tier (bandwidth upgrades reduce time)" },
                { "crack",    "crack <WEP|WPA|WPA2> <SSID>              — crack a network (CPU + GPU upgrades reduce time)" },
                { "firewall", "firewall <disable|enable> [<IP>]          — toggle device firewall; omit IP to select interactively" },
                { "show",     "show <networks|ips|locations>             — list known networks, infected IPs, or all locations" },
                { "inject",   "inject [<type> [<IP> <SSID>]]            — inject malware; miner yield scales with target CPU tier (bandwidth upgrades reduce time)" },
                { "ls",       "ls [<IP>]                                 — list files on a device; omit IP to select interactively" },
                { "copy",     "copy <filename> [<IP>]                    — copy file from device; omit IP to select interactively" },
                { "move",     "move                                      — discover a new location; move <name> — travel to a known location" },
                { "forget",   "forget network <SSID> [at <location>]    — remove network and its IPs; forget ip <IP> — remove a single device" },
            };

        private readonly CommandParser _parser;
        private readonly StringBuilder _sb = new StringBuilder(512);

        public HelpCommand(CommandParser parser)
        {
            _parser = parser;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length > 0)
                return DetailFor(args[0]);

            return ListAll();
        }

        private CommandResult ListAll()
        {
            _sb.Clear();
            _sb.AppendLine("Available commands:");
            _sb.AppendLine();

            IReadOnlyList<string> verbs = _parser.GetRegisteredVerbs();
            for (int i = 0; i < verbs.Count; i++)
            {
                string verb = verbs[i];
                string line = s_usage.TryGetValue(verb, out string desc)
                    ? $"  {desc}"
                    : $"  {verb}";
                _sb.AppendLine(line);
            }

            return CommandResult.Ok(_sb.ToString());
        }

        private CommandResult DetailFor(string verb)
        {
            if (s_usage.TryGetValue(verb, out string desc))
                return CommandResult.Ok(desc);

            return CommandResult.Fail($"No help entry for '{verb}'.");
        }
    }
}
