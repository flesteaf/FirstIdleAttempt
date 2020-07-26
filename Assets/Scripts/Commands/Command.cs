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

        public abstract void Execute(SceneManager game, CommandLine command);
        public CommandOptions GetOptionFromCommand(CommandLine command)
        {
            if (command.Option == CommandOptions.None)
            {
                return command.Option;
            }

            if (!Options.Contains(command.Option))
            {
                return CommandOptions.Invalid;
            }

            return command.Option;
        }
    }
}