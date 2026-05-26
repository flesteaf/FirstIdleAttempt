using System.Collections.Generic;

namespace HackYourWay.Models
{
    /// <summary>
    /// Top-level serialisable aggregate written to
    /// <c>OS.GetUserDataDir()/save.json</c> via System.Text.Json.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        /// <summary>Schema version; increment when adding breaking fields.</summary>
        public int Version = 1;

        public Player Player = new Player();

        /// <summary>All locations the player has visited, with nested network/device state.</summary>
        public List<LocationSaveData> Locations = new List<LocationSaveData>();

        /// <summary>Sequential ID of the next location to generate when the player moves.</summary>
        public int NextLocationId = 1;

        /// <summary>Name of the location the player was at on last save; restored on reload.</summary>
        public string CurrentLocationName;
    }

    [System.Serializable]
    public class LocationSaveData
    {
        public string Id;
        /// <summary>Human-readable location name (e.g. "node_77"); added in schema v2.</summary>
        public string Name;
        public string ConfigId;
        public bool   IsVisited;
        public List<NetworkSaveData> Networks = new List<NetworkSaveData>();
    }

    [System.Serializable]
    public class NetworkSaveData
    {
        public string        Ssid;
        public SecurityLevel SecurityLevel;
        public bool          IsHacked;
        public bool          IsScanned;
        public List<DeviceSaveData> Devices = new List<DeviceSaveData>();
    }

    [System.Serializable]
    public class DeviceSaveData
    {
        public string         Ip;
        public string         Mac;
        public FirewallStatus FirewallStatus;
        public List<int>      OpenPorts    = new List<int>();
        public Malware        ActiveMalware;
        public bool           HasMalware;
        public bool           IsScanned;
        public List<DeviceFile> Files      = new List<DeviceFile>();

        /// <summary>Device CPU tier (1–5). Defaults to 0; v2→v3 migration corrects to 1.</summary>
        public int CpuTier;

        /// <summary>Device bandwidth tier (1–5). Defaults to 0; v2→v3 migration corrects to 1.</summary>
        public int BandwidthTier;

        /// <summary>Device GPU tier (0 = none, 1–3). 0 is the correct default — no migration needed.</summary>
        public int GpuTier;
    }
}
