namespace HackYourWay.Models
{
    /// <summary>Lightweight metadata for a single save slot; stored in <c>save_index.json</c>.</summary>
    [System.Serializable]
    public class SaveSlotInfo
    {
        /// <summary>Slot number (1–7); invariant: <c>SaveSlotIndex.Slots[i].SlotNumber == i + 1</c>.</summary>
        public int SlotNumber;

        /// <summary>True when a save file exists for this slot.</summary>
        public bool IsOccupied;

        /// <summary><c>DateTime.UtcNow.Ticks</c> at the moment of last save; 0 when slot is empty.</summary>
        public long SavedAtUtcTicks;
    }

    /// <summary>
    /// Root wrapper for all 7 slot metadata entries; serialised to
    /// <c>OS.GetUserDataDir()/save_index.json</c> via System.Text.Json.
    /// </summary>
    [System.Serializable]
    public class SaveSlotIndex
    {
        /// <summary>
        /// Metadata for all 7 slots. <c>Slots[0]</c> = slot 1, <c>Slots[6]</c> = slot 7.
        /// Always length 7; <c>Slots[i].SlotNumber == i + 1</c> for all i.
        /// </summary>
        public SaveSlotInfo[] Slots = new SaveSlotInfo[7];
    }
}
