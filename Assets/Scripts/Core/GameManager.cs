using System.Collections.Generic;
using UnityEngine;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;

namespace HackYourWay.Core
{
    /// <summary>
    /// Singleton MonoBehaviour that bootstraps the game session.
    /// Loads save data, applies offline income, and wires up all services.
    /// Saves on application quit and pause.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── Public accessors ─────────────────────────────────────────────────

        /// <summary>The loaded (or default) save data for this session.</summary>
        public SaveData SaveData { get; private set; }

        /// <summary>The active player from the current save.</summary>
        public Player Player => SaveData.Player;

        /// <summary>The command parser; commands are registered in Awake.</summary>
        public CommandParser CommandParser { get; private set; }

        /// <summary>Manages all locations (procedural generation + save/restore).</summary>
        public Services.LocationService LocationService { get; private set; }

        // ── Serialized references ────────────────────────────────────────────

        [SerializeField] private TickManager _tickManager;

        // ── Private fields ───────────────────────────────────────────────────

        private SaveSystem _saveSystem;

        // ── Unity lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Bootstrap();
        }

        private void OnApplicationQuit()
        {
            PersistSession();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                PersistSession();
        }

        // ── Bootstrap ────────────────────────────────────────────────────────

        private void Bootstrap()
        {
            _saveSystem = new SaveSystem();

            // 1. Load save (returns default if no file exists).
            SaveData = _saveSystem.Load();

            // 2. Apply offline income for all income-generating malware.
            ApplyOfflineIncome();

            // 3. Restore location service state from save.
            LocationService = new Services.LocationService(FindLocationConfig());
            if (SaveData.Locations.Count > 0)
                LocationService.RestoreFromSave(SaveData.Locations, SaveData.NextLocationId);

            // 4. Build command parser; commands registered by RegisterCommands().
            CommandParser = new CommandParser();
            RegisterCommands();

            // 5. Create IncomeService and register with TickManager (T035/T037).
            var incomeService = new IncomeService(SaveData.Player, LocationService);
            if (_tickManager != null)
                _tickManager.Register(incomeService);
        }

        // ── Offline income ───────────────────────────────────────────────────

        private void ApplyOfflineIncome()
        {
            var malware = CollectAllMalware();
            if (malware.Count == 0)
                return;

            List<CurrencyBalance> gains = OfflineIncomeCalculator.Calculate(
                SaveData.Player.LastSaveUtcTicks,
                malware,
                System.DateTime.UtcNow.Ticks);

            for (int i = 0; i < gains.Count; i++)
                SaveData.Player.AddBalance(gains[i].Currency, gains[i].Amount);
        }

        private List<Malware> CollectAllMalware()
        {
            var list = new List<Malware>();
            var locs = SaveData.Locations;

            for (int l = 0; l < locs.Count; l++)
            {
                var nets = locs[l].Networks;
                for (int n = 0; n < nets.Count; n++)
                {
                    var devs = nets[n].Devices;
                    for (int d = 0; d < devs.Count; d++)
                    {
                        if (devs[d].HasMalware && devs[d].ActiveMalware != null)
                            list.Add(devs[d].ActiveMalware);
                    }
                }
            }

            return list;
        }

        // ── Command registration ─────────────────────────────────────────────

        /// <summary>
        /// Registers all built-in commands with the CommandParser.
        /// US1 commands: scan, crack, firewall, show, inject.
        /// </summary>
        private void RegisterCommands()
        {
            Player p = SaveData.Player;

            CommandParser.Register("scan",     new ScanCommand(p, LocationService));
            CommandParser.Register("crack",    new CrackCommand(p, LocationService));
            CommandParser.Register("firewall", new FirewallCommand(p));
            CommandParser.Register("show",     new ShowCommand(p, LocationService));
            CommandParser.Register("inject",   new InjectCommand(p, LocationService));
            CommandParser.Register("ls",       new LsCommand(p));
            CommandParser.Register("copy",     new CopyCommand(p));
            // Registered last so GetRegisteredVerbs() returns the complete list.
            CommandParser.Register("help",     new HelpCommand(CommandParser));
        }

        // ── Save ─────────────────────────────────────────────────────────────

        private void PersistSession()
        {
            // Flush the current location cache back to SaveData before writing.
            SaveData.Locations    = LocationService.ToSaveData();
            SaveData.NextLocationId = LocationService.NextLocationId;
            _saveSystem.Save(SaveData);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Finds a LocationConfigSO in the project (loaded via Resources).
        /// Designers should place a "LocationConfig" asset in Assets/Resources/Locations/.
        /// </summary>
        private static Data.LocationConfigSO FindLocationConfig()
        {
            var config = Resources.Load<Data.LocationConfigSO>("Locations/LocationConfig");
            if (config == null)
            {
                // Fallback: create a runtime instance with default values so
                // the game still boots in bare test scenes.
                config = ScriptableObject.CreateInstance<Data.LocationConfigSO>();
                Debug.LogWarning("[GameManager] No LocationConfig asset found in Resources/Locations/. " +
                                 "Using default values. Create Assets/Resources/Locations/LocationConfig.asset.");
            }
            return config;
        }

        // ── Test support ─────────────────────────────────────────────────────

#if UNITY_INCLUDE_TESTS
        /// <summary>
        /// Test-only entry point: force-saves then re-loads save data to simulate
        /// a session close/reopen without calling Application.Quit().
        /// Used by T043 (offline income integration test).
        /// </summary>
        public void SimulateLoad()
        {
            PersistSession();
            SaveData = _saveSystem.Load();
            ApplyOfflineIncome();
        }
#endif
    }
}
