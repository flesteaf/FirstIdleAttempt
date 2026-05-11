using System;
using HackYourWay.Core;
using HackYourWay.Interfaces;

namespace HackYourWay.Services.Commands
{
    /// <summary>Implements <c>load &lt;slot&gt;</c> — restores a previously saved game from a numbered slot.</summary>
    public class LoadCommand : ICommand
    {
        private readonly SlotSaveSystem _slotSystem;
        private readonly Action<int>    _loadSlot;

        /// <summary>
        /// Creates a <see cref="LoadCommand"/>.
        /// </summary>
        /// <param name="slotSystem">Multi-slot file I/O service.</param>
        /// <param name="loadSlot">Callback that triggers <c>GameManager.LoadSlot(int)</c>.</param>
        public LoadCommand(SlotSaveSystem slotSystem, Action<int> loadSlot)
        {
            _slotSystem = slotSystem;
            _loadSlot   = loadSlot;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Usage: load <slot>");

            if (!TryParseSlot(args[0], out int slot))
                return CommandResult.Fail("Slot must be between 1 and 7.");

            if (!_slotSystem.LoadIndex().Slots[slot - 1].IsOccupied)
                return CommandResult.Fail($"Slot {slot} is empty.");

            _loadSlot(slot);
            return CommandResult.Ok($"Game loaded from slot {slot}.");
        }

        private static bool TryParseSlot(string raw, out int slot) =>
            int.TryParse(raw, out slot) && slot >= 1 && slot <= 7;
    }
}
