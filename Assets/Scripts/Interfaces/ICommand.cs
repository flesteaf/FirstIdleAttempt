namespace HackYourWay.Interfaces
{
    /// <summary>Result returned by every terminal command execution.</summary>
    public readonly struct CommandResult
    {
        public readonly bool Success;
        public readonly string Message;

        public CommandResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static CommandResult Ok(string message) => new CommandResult(true, message);
        public static CommandResult Fail(string message) => new CommandResult(false, $"Error: {message}");
    }

    /// <summary>Contract for all player-facing terminal commands.</summary>
    public interface ICommand
    {
        /// <summary>Executes the command with the given tokenised arguments.</summary>
        /// <param name="args">Tokens after the command verb (may be empty).</param>
        CommandResult Execute(string[] args);

        /// <summary>
        /// Returns the effective execution time in seconds for this command.
        /// Instant commands (show, help, forget, move) inherit this default and return 0.
        /// Implementors MUST NOT modify state — this is a pure read-only latency query.
        /// </summary>
        float GetLatency(string[] args) => 0f;
    }
}
