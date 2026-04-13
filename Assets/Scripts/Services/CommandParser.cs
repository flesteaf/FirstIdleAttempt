using System.Collections.Generic;
using HackYourWay.Interfaces;

namespace HackYourWay.Services
{
    /// <summary>
    /// Tokenises player terminal input and dispatches to the registered
    /// <see cref="ICommand"/> handler. No LINQ — uses for-loops to avoid
    /// GC allocations in the parse hot path.
    /// </summary>
    public class CommandParser
    {
        private readonly Dictionary<string, ICommand> _registry =
            new Dictionary<string, ICommand>(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>Registers a command verb and its handler.</summary>
        public void Register(string verb, ICommand command)
        {
            _registry[verb.ToLowerInvariant()] = command;
        }

        /// <summary>
        /// Parses and executes a raw input string.
        /// Returns an error result for empty or unrecognised input.
        /// </summary>
        public CommandResult Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return CommandResult.Fail("No command entered. Type 'help' for a list.");

            // Split without LINQ — Split allocates a single array, which is acceptable.
            string[] tokens = input.Trim().Split(' ');

            string verb = tokens[0].ToLowerInvariant();

            if (!_registry.TryGetValue(verb, out ICommand command))
                return CommandResult.Fail($"Unknown command: '{verb}'. Type 'help' for a list.");

            // Build args array (everything after the verb).
            int argCount = tokens.Length - 1;
            string[] args = argCount > 0 ? new string[argCount] : System.Array.Empty<string>();
            for (int i = 0; i < argCount; i++)
                args[i] = tokens[i + 1];

            return command.Execute(args);
        }
    }
}
