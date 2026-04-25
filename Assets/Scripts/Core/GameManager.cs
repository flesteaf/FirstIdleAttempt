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

        // ── Public accessors ─────────────────────────────────────────────────

        /// <summary>The loaded (or default) save data for this session.</summary>
        public SaveData SaveData { get; private set; }

        /// <summary>The active player from the current save.</summary>
        public Player Player => SaveData.Player;

        /// <summary>The command parser; commands are registered in Awake.</summary>
        public CommandParser CommandParser { get; private set; }

        /// <summary>Manages all locations (procedural generation + save/restore).</summary>
        public Services.LocationService LocationService { get; private set; }

        /// <summary>Per-command latency calculator; uses the current player's hardware tiers.</summary>
        public Services.CommandLatencyService CommandLatencyService { get; private set; }

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

            // 4. Build latency service using the loaded player's hardware tiers.
            CommandLatencyService = new Services.CommandLatencyService(SaveData.Player);

            // 5. Restore location service state from save.
            LocationService = new Services.LocationService(FindLocationConfig());
            if (SaveData.Locations.Count > 0)
            {
                LocationService.RestoreFromSave(SaveData.Locations, SaveData.NextLocationId);
                if (!string.IsNullOrEmpty(SaveData.CurrentLocationName))
                    LocationService.SetCurrentLocation(SaveData.CurrentLocationName);
            }

            // 6. Build command parser; commands registered by RegisterCommands().
            CommandParser = new CommandParser();
            RegisterCommands();

            // 7. Create IncomeService and register with TickManager.
            var incomeService = new IncomeService(SaveData.Player, LocationService);
            if (_tickManager != null)
                _tickManager.Register(incomeService);
        }

        // ── Schema migration ─────────────────────────────────────────────────

        private void MigrateIfNeeded()
        {
            if (SaveData.Version < 2)
            {
                // v1 → v2: back-fill location names; set CurrentLocationName.
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

            if (SaveData.Version < 3)
            {
                // v2 → v3: back-fill player hardware tiers and device hardware tiers.
                // JsonUtility defaults missing int fields to 0; valid tier minimum is 1.
                var p = SaveData.Player;
                if (p.CpuTier       == 0) p.CpuTier       = 1;
                if (p.BandwidthTier == 0) p.BandwidthTier  = 1;
                // GpuTier 0 = no GPU — correct default, no migration needed.

                for (int l = 0; l < SaveData.Locations.Count; l++)
                {
                    var nets = SaveData.Locations[l].Networks;
                    for (int n = 0; n < nets.Count; n++)
                    {
                        var devs = nets[n].Devices;
                        for (int d = 0; d < devs.Count; d++)
                        {
                            if (devs[d].CpuTier       == 0) devs[d].CpuTier       = 1;
                            if (devs[d].BandwidthTier == 0) devs[d].BandwidthTier  = 1;
                        }
                    }
                }

                SaveData.Version = 3;
                _saveSystem.Save(SaveData);
            }
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

            CommandParser.Register("scan",     new ScanCommand(p, LocationService, CommandLatencyService));
            CommandParser.Register("crack",    new CrackCommand(p, LocationService, CommandLatencyService));
            CommandParser.Register("firewall", new FirewallCommand(p, LocationService, CommandLatencyService));
            CommandParser.Register("show",     new ShowCommand(LocationService));
            CommandParser.Register("inject",   new InjectCommand(p, LocationService, CommandLatencyService));
            CommandParser.Register("ls",       new LsCommand(LocationService, CommandLatencyService));
            CommandParser.Register("copy",     new CopyCommand(p, LocationService, CommandLatencyService));
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
