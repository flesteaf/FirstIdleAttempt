using System.Collections.Generic;
using Godot;
using HackYourWay.Models;
using HackYourWay.Services;
using HackYourWay.Services.Commands;

namespace HackYourWay.Core
{
    /// <summary>
    /// Autoload singleton that bootstraps the game session.
    /// Loads save data, applies offline income, runs schema migrations, and wires all services.
    /// Saves on application quit and focus-out.
    /// </summary>
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        // ── Public accessors ─────────────────────────────────────────────────

        public SaveData SaveData { get; private set; }
        public Player Player => SaveData.Player;
        public CommandParser CommandParser { get; private set; }
        public Services.LocationService LocationService { get; private set; }
        public Services.CommandLatencyService CommandLatencyService { get; private set; }
        public SlotSaveSystem SlotSaveSystem { get; private set; }
        public Services.ConfirmationService ConfirmationService { get; private set; }

        /// <summary>Active save slot (1–7), or -1 when no slot is active (e.g. after <c>newgame</c>).</summary>
        public int CurrentSlot { get; private set; } = -1;

        // ── Private fields ───────────────────────────────────────────────────

        private SaveSystem   _saveSystem;
        private TickManager  _tickManager;

        // ── Godot lifecycle ──────────────────────────────────────────────────

        public override void _Ready()
        {
            Instance = this;
            GetTree().AutoAcceptQuit = false;

            _tickManager = new TickManager();
            AddChild(_tickManager);

            Bootstrap();
        }

        public override void _Notification(int what)
        {
            switch (what)
            {
                case (int)NotificationWMCloseRequest:
                    PersistSession();
                    GetTree().Quit();
                    break;
                case (int)NotificationApplicationFocusOut:
                    PersistSession();
                    break;
            }
        }

        // ── Bootstrap ────────────────────────────────────────────────────────

        private void Bootstrap()
        {
            _saveSystem         = new SaveSystem();
            SlotSaveSystem      = new SlotSaveSystem();
            ConfirmationService = new Services.ConfirmationService();

            // 1. One-time legacy migration: copy save.json → save_1.json if first boot with slot system.
            bool migrated = SlotSaveSystem.MigrateLegacyIfNeeded();
            if (migrated)
            {
                CurrentSlot = 1;
                SaveData    = SlotSaveSystem.LoadSlot(1) ?? _saveSystem.Load();
            }
            else
            {
                CurrentSlot = -1;
                SaveData    = _saveSystem.Load();
            }

            // 2. Run schema migration (v1 → v3) before anything reads location data.
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
            _tickManager.Register(incomeService);
        }

        // ── Schema migration ─────────────────────────────────────────────────

        private void MigrateIfNeeded()
        {
            if (SaveData.Version < 2)
            {
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
                var p = SaveData.Player;
                if (p.CpuTier       == 0) p.CpuTier       = 1;
                if (p.BandwidthTier == 0) p.BandwidthTier  = 1;

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

            CommandParser.Register("save",    new SaveCommand(SlotSaveSystem, ConfirmationService,
                                                  () => SaveData, slot => CurrentSlot = slot));
            CommandParser.Register("load",    new LoadCommand(SlotSaveSystem, slot => LoadSlot(slot)));
            CommandParser.Register("delsave", new DelsaveCommand(SlotSaveSystem,
                                                  slot => { if (CurrentSlot == slot) CurrentSlot = -1; }));
            CommandParser.Register("newgame", new NewGameCommand(ConfirmationService, NewGame));
            CommandParser.Register("saves",   new SavesCommand(SlotSaveSystem));

            CommandParser.Register("help",    new HelpCommand(CommandParser));
        }

        // ── Slot operations ──────────────────────────────────────────────────

        /// <summary>
        /// Loads save data from <paramref name="slot"/>, applies schema migration, re-initialises all
        /// services and command instances without a scene reload. Sets <see cref="CurrentSlot"/>.
        /// </summary>
        public void LoadSlot(int slot)
        {
            SaveData              = SlotSaveSystem.LoadSlot(slot) ?? new SaveData();
            MigrateIfNeeded();
            CommandLatencyService = new Services.CommandLatencyService(SaveData.Player);
            LocationService       = new Services.LocationService(FindLocationConfig());
            if (SaveData.Locations.Count > 0)
            {
                LocationService.RestoreFromSave(SaveData.Locations, SaveData.NextLocationId);
                if (!string.IsNullOrEmpty(SaveData.CurrentLocationName))
                    LocationService.SetCurrentLocation(SaveData.CurrentLocationName);
            }
            RegisterCommands();
            CurrentSlot = slot;
        }

        /// <summary>
        /// Resets the game to the default initial state. Existing saves are preserved.
        /// Sets <see cref="CurrentSlot"/> to -1; <see cref="PersistSession"/> becomes a no-op
        /// until the player explicitly saves to a slot.
        /// </summary>
        public void NewGame()
        {
            SaveData              = CreateDefaultSaveData();
            CommandLatencyService = new Services.CommandLatencyService(SaveData.Player);
            LocationService       = new Services.LocationService(FindLocationConfig());
            RegisterCommands();
            CurrentSlot = -1;
        }

        // ── Save ─────────────────────────────────────────────────────────────

        private void PersistSession()
        {
            if (CurrentSlot == -1) return;
            SaveData.Locations           = LocationService.ToSaveData();
            SaveData.NextLocationId      = LocationService.NextLocationId;
            SaveData.CurrentLocationName = LocationService.GetCurrentLocationName();
            SlotSaveSystem.SaveSlot(CurrentSlot, SaveData);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static SaveData CreateDefaultSaveData()
        {
            var data = new SaveData();
            data.Player.UnlockedCurrencies.Add(CurrencyType.Bitcoin);
            data.Player.AddBalance(CurrencyType.Bitcoin, 0);
            return data;
        }

        private static Data.LocationConfigSO FindLocationConfig()
        {
            var config = GD.Load<Data.LocationConfigSO>("res://resources/locations/LocationConfig.tres");
            if (config == null)
            {
                GD.PushWarning("[GameManager] No LocationConfig.tres found at res://resources/locations/. " +
                               "Using default values. Create the asset in the Godot editor.");
                config = new Data.LocationConfigSO();
            }
            return config;
        }
    }
}
