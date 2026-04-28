using System;
using HackYourWay.Core;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>Implements <c>save &lt;slot&gt;</c> — saves current game state to a numbered slot (1–7).</summary>
    public class SaveCommand : ICommand
    {
        private readonly SlotSaveSystem      _slotSystem;
        private readonly ConfirmationService _confirmService;
        private readonly Func<SaveData>      _getCurrentData;
        private readonly Action<int>         _setActiveSlot;

        /// <summary>
        /// Creates a <see cref="SaveCommand"/>.
        /// </summary>
        /// <param name="slotSystem">Multi-slot file I/O service.</param>
        /// <param name="confirmService">Pending-confirmation state machine.</param>
        /// <param name="getCurrentData">Provider for the current session's <see cref="SaveData"/>.</param>
        /// <param name="setActiveSlot">Callback to update <c>GameManager.CurrentSlot</c>.</param>
        public SaveCommand(
            SlotSaveSystem      slotSystem,
            ConfirmationService confirmService,
            Func<SaveData>      getCurrentData,
            Action<int>         setActiveSlot)
        {
            _slotSystem     = slotSystem;
            _confirmService = confirmService;
            _getCurrentData = getCurrentData;
            _setActiveSlot  = setActiveSlot;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Usage: save <slot>");

            if (!TryParseSlot(args[0], out int slot))
                return CommandResult.Fail("Slot must be between 1 and 7.");

            if (_slotSystem.LoadIndex().Slots[slot - 1].IsOccupied)
            {
                string prompt = $"Slot {slot} is occupied, overwrite? (y/n)";
                _confirmService.RequestConfirmation(
                    prompt,
                    onConfirm: () =>
                    {
                        _slotSystem.SaveSlot(slot, _getCurrentData());
                        _setActiveSlot(slot);
                        UI.TerminalController.Instance?.AppendOutput($"Game saved to slot {slot}.");
                    },
                    onCancel: () => UI.TerminalController.Instance?.AppendOutput("Save cancelled."));
                return CommandResult.Ok(prompt);
            }

            _slotSystem.SaveSlot(slot, _getCurrentData());
            _setActiveSlot(slot);
            return CommandResult.Ok($"Game saved to slot {slot}.");
        }

        private static bool TryParseSlot(string raw, out int slot) =>
            int.TryParse(raw, out slot) && slot >= 1 && slot <= 7;
    }
}
