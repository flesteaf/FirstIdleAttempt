using System;

namespace Assets.Scripts.Commands
{
    public static class CommandLineExtensions
    {
        public static bool HasArgument(this CommandLine command)
        {
            return !string.IsNullOrEmpty(command.Argument);
        }

        public static bool HasOption(this CommandLine command)
        {
            return command.Option != CommandOptions.None;
        }

        public static bool HasArgumentAndOption(this CommandLine command)
        {
            return command.HasArgument() && command.HasOption();
        }

        public static bool HasArgumentAndNoOption(this CommandLine command)
        {
            return command.HasArgument() && !command.HasOption();
        }

        public static CommandOptions ArgumentAsOption(this CommandLine command)
        {
            if (Enum.TryParse(command.Argument, out CommandOptions option))
            {
                return option;
            }

            return CommandOptions.Invalid;
        }
    }
}
