using System;
using System.IO;
using UnityEngine;
using HackYourWay.Models;

namespace HackYourWay.Core
{
    /// <summary>
    /// Owns all multi-slot file I/O. Wraps path construction and atomic index updates.
    /// All public methods validate slot is in [1, 7]; violations throw <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public class SlotSaveSystem
    {
        private readonly string _basePath;

        /// <summary>Creates a <see cref="SlotSaveSystem"/> targeting <c>Application.persistentDataPath</c>.</summary>
        public SlotSaveSystem() : this(Application.persistentDataPath) { }

        /// <summary>Creates a <see cref="SlotSaveSystem"/> targeting a custom base path (for test isolation).</summary>
        public SlotSaveSystem(string basePath)
        {
            _basePath = basePath;
        }

        /// <summary>
        /// Reads <c>save_{slot}.json</c> and deserialises it.
        /// Returns <c>null</c> if the file is absent.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Slot is outside [1, 7].</exception>
        public SaveData LoadSlot(int slot)
        {
            ValidateSlot(slot);
            string path = SlotPath(slot);
            if (!File.Exists(path))
                return null;
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }

        /// <summary>
        /// Writes <c>save_{slot}.json</c> (full <see cref="SaveData"/>) and updates the index entry.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Slot is outside [1, 7].</exception>
        public void SaveSlot(int slot, SaveData data)
        {
            ValidateSlot(slot);
            File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(data, prettyPrint: false));

            SaveSlotIndex index = LoadIndex();
            index.Slots[slot - 1].IsOccupied      = true;
            index.Slots[slot - 1].SavedAtUtcTicks  = DateTime.UtcNow.Ticks;
            SaveIndex(index);
        }

        /// <summary>
        /// Deletes <c>save_{slot}.json</c> and marks the index entry as empty.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Slot is outside [1, 7].</exception>
        public void DeleteSlot(int slot)
        {
            ValidateSlot(slot);
            string path = SlotPath(slot);
            if (File.Exists(path))
                File.Delete(path);

            SaveSlotIndex index = LoadIndex();
            index.Slots[slot - 1].IsOccupied      = false;
            index.Slots[slot - 1].SavedAtUtcTicks  = 0;
            SaveIndex(index);
        }

        /// <summary>
        /// Reads <c>save_index.json</c>.
        /// Returns a fresh default 7-entry index (all empty) if the file is absent or corrupt.
        /// </summary>
        public SaveSlotIndex LoadIndex()
        {
            string path = IndexPath();
            if (!File.Exists(path))
                return CreateDefaultIndex();
            string         json  = File.ReadAllText(path);
            SaveSlotIndex  index = JsonUtility.FromJson<SaveSlotIndex>(json);
            if (index == null || index.Slots == null || index.Slots.Length != 7)
                return CreateDefaultIndex();
            return index;
        }

        /// <summary>Writes <c>save_index.json</c>.</summary>
        public void SaveIndex(SaveSlotIndex index)
        {
            File.WriteAllText(IndexPath(), JsonUtility.ToJson(index, prettyPrint: false));
        }

        /// <summary>
        /// Copies <c>save.json</c> to <c>save_1.json</c> on first boot when no slot files exist.
        /// Returns <c>true</c> if migration ran; caller should then set <c>GameManager.CurrentSlot = 1</c>.
        /// The legacy file is kept as a backup.
        /// </summary>
        public bool MigrateLegacyIfNeeded()
        {
            string legacyPath = Path.Combine(_basePath, "save.json");
            if (!File.Exists(legacyPath))
                return false;

            for (int s = 1; s <= 7; s++)
            {
                if (File.Exists(SlotPath(s)))
                    return false;
            }

            File.Copy(legacyPath, SlotPath(1), overwrite: false);

            long ticks = DateTime.UtcNow.Ticks;
            try
            {
                SaveData legacy = JsonUtility.FromJson<SaveData>(File.ReadAllText(legacyPath));
                if (legacy != null && legacy.Player.LastSaveUtcTicks > 0)
                    ticks = legacy.Player.LastSaveUtcTicks;
            }
            catch { /* fall back to UtcNow */ }

            SaveSlotIndex index = CreateDefaultIndex();
            index.Slots[0].IsOccupied      = true;
            index.Slots[0].SavedAtUtcTicks  = ticks;
            SaveIndex(index);

            return true;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private string SlotPath(int slot)  => Path.Combine(_basePath, $"save_{slot}.json");
        private string IndexPath()         => Path.Combine(_basePath, "save_index.json");

        private static void ValidateSlot(int slot)
        {
            if (slot < 1 || slot > 7)
                throw new ArgumentOutOfRangeException(nameof(slot), slot, "Slot must be between 1 and 7.");
        }

        private static SaveSlotIndex CreateDefaultIndex()
        {
            var index = new SaveSlotIndex();
            for (int i = 0; i < 7; i++)
                index.Slots[i] = new SaveSlotInfo { SlotNumber = i + 1, IsOccupied = false, SavedAtUtcTicks = 0 };
            return index;
        }
    }
}
