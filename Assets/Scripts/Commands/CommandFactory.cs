using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Commands
{
    internal static class CommandFactory
    {
        private static readonly Dictionary<string, Command> commands;

        static CommandFactory()
        {
            HelpCommand helpCommand = new HelpCommand();
            StatusCommand statusCommand = new StatusCommand();

            commands = new Dictionary<string, Command>
            {
                { helpCommand.Name, helpCommand },
                { statusCommand.Name, statusCommand }
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
