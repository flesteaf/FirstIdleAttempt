using System;
using System.Text;
using HackYourWay.Core;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>Implements <c>saves</c> — displays the occupancy status of all 7 save slots.</summary>
    public class SavesCommand : ICommand
    {
        private readonly SlotSaveSystem _slotSystem;
        private readonly StringBuilder  _sb = new StringBuilder(256);

        /// <summary>Creates a <see cref="SavesCommand"/>.</summary>
        /// <param name="slotSystem">Multi-slot file I/O service.</param>
        public SavesCommand(SlotSaveSystem slotSystem)
        {
            _slotSystem = slotSystem;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            SaveSlotIndex index = _slotSystem.LoadIndex();
            _sb.Clear();

            for (int i = 0; i < 7; i++)
            {
                SaveSlotInfo info  = index.Slots[i];
                string       value = info.IsOccupied
                    ? new DateTime(info.SavedAtUtcTicks, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm")
                    : "[empty]";
                _sb.AppendLine($"Slot {info.SlotNumber}: {value}");
            }

            return CommandResult.Ok(_sb.ToString());
        }
    }
}
