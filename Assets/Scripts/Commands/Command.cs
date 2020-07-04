using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Commands
{
    public abstract class Command
    {
        //TODO: implement command help option and CanExecute
        public abstract CommandNames Name { get; }
        public abstract List<CommandOptions> Options { get; }

        //public abstract string Description { get; }
        protected const string HelpOption = "?";

        public abstract void Execute(GameManager game, string command);
        public CommandOptions GetOptionFromCommand(string command)
        {
            string[] commandComponents = command.Split(new[] { '\'', '"', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commandComponents.Length < 2)
            {
                return CommandOptions.None;
            }

            return Options.FirstOrDefault(x => x.ToString().Equals(commandComponents[1]));
        }
    }
}