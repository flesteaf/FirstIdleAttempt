using System;

namespace Assets.Scripts.Commands
{
    public class CommandLine
    {
        private readonly CommandNames commandName;
        private readonly CommandOptions option = CommandOptions.None;
        private readonly string originalCommand;

        public CommandNames CommandName { get => commandName; }
        public CommandOptions Option { get => option; }
        public bool LongArgument { get; set; }
        public string Argument { get; private set; }

        public CommandLine(string command)
        {
            if (string.IsNullOrEmpty(command)) throw new ArgumentNullException($"{nameof(command)} should not be null or empty");
            originalCommand = command;

            string[] segments = command.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
            string[] commandComponents = segments[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (!Enum.TryParse(commandComponents[0], out commandName))
            {
                commandName = CommandNames.invalid;
                return;
            }

            int componentsLength = commandComponents.Length;
            if (segments.Length == 2)
            {
                Argument = segments[1];
                LongArgument = true;

                if (componentsLength > 1 && !Enum.TryParse(commandComponents[1], out option))
                {
                    throw new ArgumentException($"Command option {option} is unsupported for any command");
                }
            }
            else
            {
                if (componentsLength > 1) Argument = commandComponents[componentsLength - 1];
                if (componentsLength > 2 && !Enum.TryParse(commandComponents[1], out option))
                {
                    throw new ArgumentException($"Command option {option} is unsupported for any command");
                }
            }
        }

        #region Overrides

        public override string ToString()
        {
            return originalCommand;
        }

        #endregion Overrides
    }

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
