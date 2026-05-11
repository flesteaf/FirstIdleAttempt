using System;
using HackYourWay.Core;
using HackYourWay.Interfaces;

namespace HackYourWay.Services.Commands
{
    /// <summary>Implements <c>delsave &lt;slot&gt;</c> — removes an existing save from a numbered slot.</summary>
    public class DelsaveCommand : ICommand
    {
        private readonly SlotSaveSystem _slotSystem;
        private readonly Action<int>    _resetCurrentSlotIfActive;

        /// <summary>
        /// Creates a <see cref="DelsaveCommand"/>.
        /// </summary>
        /// <param name="slotSystem">Multi-slot file I/O service.</param>
        /// <param name="resetCurrentSlotIfActive">
        /// Callback invoked with the deleted slot number. Caller clears <c>GameManager.CurrentSlot</c>
        /// when the deleted slot matches the active slot.
        /// </param>
        public DelsaveCommand(SlotSaveSystem slotSystem, Action<int> resetCurrentSlotIfActive)
        {
            _slotSystem               = slotSystem;
            _resetCurrentSlotIfActive = resetCurrentSlotIfActive;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Usage: delsave <slot>");

            if (!TryParseSlot(args[0], out int slot))
                return CommandResult.Fail("Slot must be between 1 and 7.");

            if (!_slotSystem.LoadIndex().Slots[slot - 1].IsOccupied)
                return CommandResult.Fail($"Slot {slot} has no save to remove.");

            _slotSystem.DeleteSlot(slot);
            _resetCurrentSlotIfActive(slot);
            return CommandResult.Ok($"Save slot {slot} removed.");
        }

        private static bool TryParseSlot(string raw, out int slot) =>
            int.TryParse(raw, out slot) && slot >= 1 && slot <= 7;
    }
}
