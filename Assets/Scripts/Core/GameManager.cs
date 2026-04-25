using System.Collections.Generic;
using UnityEngine;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;

namespace HackYourWay.Core
{
    /// <summary>
    /// Singleton MonoBehaviour that bootstraps the game session.
    /// Loads save data, applies offline income, runs v1→v2 migration, and wires all services.
    /// Saves on application quit and pause.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── Command latency ──────────────────────────────────────────────────

        /// <summary>Base duration in seconds for command execution before any speed upgrades.</summary>
        public const float BaseCommandLatencySeconds = 1.0f;

        /// <summary>
        /// Returns the effective command latency for the current session.
        /// <c>BaseCommandLatencySeconds × (1 − Player.CommandSpeedUpgrade)</c>.
        /// Returns 0 when the player has full instant-execution upgrades.
        /// </summary>
        public float GetCommandLatency() =>
            BaseCommandLatencySeconds * (1f - Player.CommandSpeedUpgrade);

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

            // 2. Run schema migration (v1 → v2) before anything reads location data.
            MigrateIfNeeded();

            // 3. Apply offline income for all income-generating malware.
            ApplyOfflineIncome();

            // 4. Restore location service state from save.
            LocationService = new Services.LocationService(FindLocationConfig());
            if (SaveData.Locations.Count > 0)
            {
                LocationService.RestoreFromSave(SaveData.Locations, SaveData.NextLocationId);
                if (!string.IsNullOrEmpty(SaveData.CurrentLocationName))
                    LocationService.SetCurrentLocation(SaveData.CurrentLocationName);
            }

            // 5. Build command parser; commands registered by RegisterCommands().
            CommandParser = new CommandParser();
            RegisterCommands();

            // 6. Create IncomeService and register with TickManager.
            var incomeService = new IncomeService(SaveData.Player, LocationService);
            if (_tickManager != null)
                _tickManager.Register(incomeService);
        }

        // ── Schema migration ─────────────────────────────────────────────────

        private void MigrateIfNeeded()
        {
            if (SaveData.Version >= 2) return;

            // v1 → v2: back-fill location names from seeds; set CurrentLocationName.
            const int nameMultiplier = 77;
            const int nameModulus    = 1000;

            for (int i = 0; i < SaveData.Locations.Count; i++)
            {
                var loc = SaveData.Locations[i];
                if (string.IsNullOrEmpty(loc.Name) && int.TryParse(loc.Id, out int seed))
                    loc.Name = $"node_{seed * nameMultiplier % nameModulus}";
            }

            if (string.IsNullOrEmpty(SaveData.CurrentLocationName) && SaveData.Locations.Count > 0)
                SaveData.CurrentLocationName = SaveData.Locations[0].Name ?? "node_77";

            SaveData.Version = 2;
            _saveSystem.Save(SaveData);
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

        private void RegisterCommands()
        {
            Player p = SaveData.Player;

            CommandParser.Register("scan",     new ScanCommand(p, LocationService));
            CommandParser.Register("crack",    new CrackCommand(p, LocationService));
            CommandParser.Register("firewall", new FirewallCommand(p, LocationService));
            CommandParser.Register("show",     new ShowCommand(LocationService));
            CommandParser.Register("inject",   new InjectCommand(p, LocationService));
            CommandParser.Register("ls",       new LsCommand(LocationService));
            CommandParser.Register("copy",     new CopyCommand(p, LocationService));
            CommandParser.Register("move",     new MoveCommand(LocationService));
            CommandParser.Register("forget",   new ForgetCommand(LocationService));
            // Registered last so GetRegisteredVerbs() returns the complete list.
            CommandParser.Register("help",     new HelpCommand(CommandParser));
        }

        // ── Save ─────────────────────────────────────────────────────────────

        private void PersistSession()
        {
            SaveData.Locations           = LocationService.ToSaveData();
            SaveData.NextLocationId      = LocationService.NextLocationId;
            SaveData.CurrentLocationName = LocationService.GetCurrentLocationName();
            _saveSystem.Save(SaveData);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static Data.LocationConfigSO FindLocationConfig()
        {
            var config = Resources.Load<Data.LocationConfigSO>("Locations/LocationConfig");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<Data.LocationConfigSO>();
                Debug.LogWarning("[GameManager] No LocationConfig asset found in Resources/Locations/. " +
                                 "Using default values. Create Assets/Resources/Locations/LocationConfig.asset.");
            }
            return config;
        }

        // ── Test support ─────────────────────────────────────────────────────

#if UNITY_INCLUDE_TESTS
        public void SimulateLoad()
        {
            PersistSession();
            SaveData = _saveSystem.Load();
            ApplyOfflineIncome();
        }
#endif
    }
}
