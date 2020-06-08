using System;
using System.Collections.Generic;

namespace Assets.Scripts.Commands
{
    internal static class CommandFactory
    {
        private static readonly Dictionary<string, Command> commands;

        static CommandFactory()
        {
            HelpCommand helpCommand = new HelpCommand();
            StatusCommand statusCommand = new StatusCommand();
            CrackCommand crackCommand = new CrackCommand();
            FirewallCommand firewallCommand = new FirewallCommand();
            InjectCommand injectCommand = new InjectCommand();
            ScanCommand scanCommand = new ScanCommand();
            SetRansomwareCommand setRansomwareCommand = new SetRansomwareCommand();
            ShowCommand showCommand = new ShowCommand();
            StoreCommand storeCommand = new StoreCommand();

            commands = new Dictionary<string, Command>
            {
                { helpCommand.Name, helpCommand },
                { statusCommand.Name, statusCommand },
                { crackCommand.Name, crackCommand },
                { firewallCommand.Name, firewallCommand },
                { injectCommand.Name, injectCommand },
                { scanCommand.Name, scanCommand },
                { setRansomwareCommand.Name, setRansomwareCommand },
                { showCommand.Name, showCommand },
                { storeCommand.Name, storeCommand }
            };
        }

        internal static IEnumerable<string> GetAllCommandsName()
        {
            return commands.Keys;
        }

        internal static Command GetCommand(string command)
        {
            string commandName = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
            if (!commands.ContainsKey(commandName))
            {
                return new InvalidCommand();
            }

            return commands[commandName];
        }
    }
}