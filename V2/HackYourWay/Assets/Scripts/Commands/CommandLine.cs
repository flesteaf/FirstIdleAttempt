using Assets.Scripts.Commands.CommandSegments;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Commands
{
    public partial class CommandLine
    {
        private CommandNames commandName;
        private CommandOptions option = CommandOptions.None;
        private readonly string originalCommand;

        public CommandNames CommandName { get => commandName; }
        public CommandOptions Option { get => option; }

        /// <summary>
        /// Is true when there are 2 or more arguments to a command|command option
        /// </summary>
        public bool TooManyArguments { get; set; }
        public string Argument { get; private set; }

        public CommandLine(string command)
        {
            if (string.IsNullOrEmpty(command)) throw new ArgumentNullException($"{nameof(command)} should not be null or empty");
            originalCommand = command;

            var segments = CommandSegmentFactory.CreateSegments(command).ToList();
            if (segments.Count == 1)
            {
                AnalyzeCommand(segments[0].Segment);
                return;
            }

            AnalyzeMultiWordCommand(segments);
        }

        private void AnalyzeMultiWordCommand(List<CommandSegment> segments)
        {
            if (segments.Count > 2 || segments[0].HasQuotes)
            {
                throw new ArgumentException($"Too difficult to analyze due to too much \" and ' presence in the command. Please re-check your wording");
            }

            AnalyzeCommand(segments[0].Segment);
            Argument = segments[1].Segment;
        }

        private void AnalyzeCommand(string segment)
        {
            string[] commandComponents = segment.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (!Enum.TryParse(commandComponents[0], out commandName))
            {
                commandName = CommandNames.invalid;
                return;
            }

            if (commandComponents.Length == 2)
            {
                //If the are only 2 elements in the received segment, the second element is either the option or the argument
                if (!Enum.TryParse(commandComponents[1], out option))
                {
                    Argument = commandComponents[1];
                }
            }

            if (commandComponents.Length == 3)
            {
                Argument = commandComponents[2];
                if (!Enum.TryParse(commandComponents[1], out option))
                {
                    throw new ArgumentException($"Command option {option} is unsupported for any command");
                }
            }

            if (commandComponents.Length > 3)
            {
                Argument = commandComponents[2];
                TooManyArguments = true;
                if (!Enum.TryParse(commandComponents[1], out option))
                {
                    throw new ArgumentException($"Command option {option} is unsupported for any command");
                }
            }

            return;
        }

        /*
         * Initial parsing code:
         * string[] commandComponents = segments[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

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
         */

        public override string ToString()
        {
            return originalCommand;
        }
    }
}
