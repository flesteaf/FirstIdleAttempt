using System.IO;
using UnityEngine;
using HackYourWay.Models;

namespace HackYourWay.Core
{
    /// <summary>
    /// Serialises and deserialises <see cref="SaveData"/> to a JSON file
    /// at <c>Application.persistentDataPath/save.json</c> via JsonUtility.
    /// Note: JsonUtility does not support Dictionary — Player uses
    /// List&lt;CurrencyBalance&gt; as a workaround (see data-model.md).
    /// </summary>
    public class SaveSystem
    {
        private readonly string _savePath;

        /// <summary>Creates a SaveSystem targeting the default persistent data path.</summary>
        public SaveSystem() : this(Path.Combine(Application.persistentDataPath, "save.json")) { }

        /// <summary>Creates a SaveSystem targeting a custom path (useful for tests).</summary>
        public SaveSystem(string savePath)
        {
            _savePath = savePath;
        }

        /// <summary>
        /// Loads save data from disk. Returns a fresh <see cref="SaveData"/>
        /// with Bitcoin unlocked if the file does not exist.
        /// </summary>
        public SaveData Load()
        {
            if (!File.Exists(_savePath))
                return CreateDefault();

            string json = File.ReadAllText(_savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
                return CreateDefault();

            return data;
        }

        /// <summary>Writes the current save state to disk.</summary>
        public void Save(SaveData data)
        {
            data.Player.LastSaveUtcTicks = System.DateTime.UtcNow.Ticks;
            string json = JsonUtility.ToJson(data, prettyPrint: false);
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
