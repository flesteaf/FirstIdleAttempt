using System;
using HackYourWay.Interfaces;

namespace HackYourWay.Services.Commands
{
    /// <summary>Implements <c>newgame</c> — resets the game to the default initial state after confirmation.</summary>
    public class NewGameCommand : ICommand
    {
        private readonly ConfirmationService _confirmService;
        private readonly Action              _newGame;

        private const string ConfirmPrompt = "Unsaved progress will be lost. Continue? (y/n)";

        /// <summary>
        /// Creates a <see cref="NewGameCommand"/>.
        /// </summary>
        /// <param name="confirmService">Pending-confirmation state machine.</param>
        /// <param name="newGame">Callback that triggers <c>GameManager.NewGame()</c>.</param>
        public NewGameCommand(ConfirmationService confirmService, Action newGame)
        {
            _confirmService = confirmService;
            _newGame        = newGame;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            _confirmService.RequestConfirmation(
                ConfirmPrompt,
                onConfirm: () =>
                {
                    _newGame();
                    UI.TerminalController.Instance?.AppendOutput("New game started.");
                },
                onCancel: () => UI.TerminalController.Instance?.AppendOutput("New game cancelled."));

            return CommandResult.Ok(ConfirmPrompt);
        }
    }
}
