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
        // ── Static usage catalogue ────────────────────────────────────────────

        /// <summary>
        /// One-line descriptions for every registered verb.
        /// Keep in sync with <see cref="GameManager.RegisterCommands"/>.
        /// </summary>
        private static readonly Dictionary<string, string> s_usage =
            new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "help",     "help [command]         — show this list or detail for a command" },
                { "scan",     "scan                   — discover networks at current location" },
                { "crack",    "crack <ssid>           — break into a WEP/WPA network" },
                { "firewall", "firewall <on|off>      — toggle your personal firewall" },
                { "show",     "show <networks|ips>    — display known networks or infected devices" },
                { "inject",   "inject <ip> <type>     — deploy malware onto a compromised device" },
                { "ls",       "ls                     — list items in your current directory" },
                { "copy",     "copy <src> <dst>       — copy a file between paths" },
            };

        // ── Dependencies ──────────────────────────────────────────────────────

        private readonly CommandParser _parser;
        private readonly StringBuilder _sb = new StringBuilder(512);

        public HelpCommand(CommandParser parser)
        {
            _parser = parser;
        }

        // ── ICommand ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length > 0)
                return DetailFor(args[0]);

            return ListAll();
        }

        // ── Internal ──────────────────────────────────────────────────────────

        /// <summary>Lists every registered verb with its one-line description.</summary>
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

        /// <summary>Shows the usage line for a specific verb.</summary>
        private CommandResult DetailFor(string verb)
        {
            if (s_usage.TryGetValue(verb, out string desc))
                return CommandResult.Ok(desc);

            return CommandResult.Fail($"No help entry for '{verb}'.");
        }
    }
}
