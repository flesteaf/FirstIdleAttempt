using System.IO;
using System.Text.Json;
using Godot;
using HackYourWay.Models;

namespace HackYourWay.Core
{
    /// <summary>
    /// Serialises and deserialises <see cref="SaveData"/> to a JSON file
    /// at <c>OS.GetUserDataDir()/save.json</c> via System.Text.Json.
    /// </summary>
    public class SaveSystem
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions { IncludeFields = true };

        private readonly string _savePath;

        /// <summary>Creates a SaveSystem targeting the default user data directory.</summary>
        public SaveSystem() : this(Path.Combine(OS.GetUserDataDir(), "save.json")) { }

        /// <summary>Creates a SaveSystem targeting a custom path (useful for tests).</summary>
        public SaveSystem(string savePath)
        {
            _savePath = savePath;
        }

        /// <summary>
        /// Loads save data from disk. Returns a fresh <see cref="SaveData"/>
        /// with Bitcoin unlocked if the file does not exist or is corrupt.
        /// </summary>
        public SaveData Load()
        {
            if (!File.Exists(_savePath))
                return CreateDefault();

            try
            {
                string json = File.ReadAllText(_savePath);
                SaveData data = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);
                return data ?? CreateDefault();
            }
            catch
            {
                return CreateDefault();
            }
        }

        /// <summary>Writes the current save state to disk.</summary>
        public void Save(SaveData data)
        {
            data.Player.LastSaveUtcTicks = System.DateTime.UtcNow.Ticks;
            string json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(_savePath, json);
        }

        private static SaveData CreateDefault()
        {
            var data = new SaveData();
            data.Player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            data.Player.AddBalance(CurrencyType.Bitcoin, 0);
            return data;
        }
    }
}
