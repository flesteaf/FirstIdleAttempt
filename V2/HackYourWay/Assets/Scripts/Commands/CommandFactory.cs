using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Commands
{
    internal static class CommandFactory
    {
        private static readonly Dictionary<CommandNames, Command> commands;

        static CommandFactory()
        {
            HelpCommand help = new HelpCommand();
            StatusCommand status = new StatusCommand();
            CrackCommand crack = new CrackCommand();
            FirewallCommand firewall = new FirewallCommand();
            InjectCommand inject = new InjectCommand();
            ScanCommand scan = new ScanCommand();
            SetRansomwareCommand setRansomware = new SetRansomwareCommand();
            ShowCommand show = new ShowCommand();
            StoreCommand store = new StoreCommand();
            BuyCommand buy = new BuyCommand();
            InvalidCommand invalid = new InvalidCommand();
            ClearCommand clear = new ClearCommand();

            commands = new Dictionary<CommandNames, Command>
            {
                { help.Name, help },
                { status.Name, status },
                { crack.Name, crack },
                { firewall.Name, firewall },
                { inject.Name, inject },
                { scan.Name, scan },
                { setRansomware.Name, setRansomware },
                { show.Name, show },
                { store.Name, store },
                { buy.Name, buy },
                { invalid.Name, invalid },
                { clear.Name, clear }
            };
        }

        internal static IEnumerable<string> GetAllCommandsName()
        {
            return commands.Keys.Select(x => x.ToString());
        }

        internal static Command GetCommand(CommandLine command)
        {
            return commands[command.CommandName];
        }
    }
}