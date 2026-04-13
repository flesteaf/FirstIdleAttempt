using System.Collections.Generic;
using HackYourWay.Data;
using HackYourWay.Models;

namespace HackYourWay.Services
{
    /// <summary>
    /// Generates and manages <see cref="Location"/> instances procedurally.
    /// Location IDs are sequential integers converted to strings ("1", "2", …);
    /// the integer value doubles as the PRNG seed for deterministic, reproducible generation.
    /// </summary>
    public class LocationService
    {
        private readonly LocationConfigSO _config;
        private readonly Dictionary<string, Location> _locationCache = new Dictionary<string, Location>();

        private int _currentLocationId = 1;
        private int _nextLocationId    = 1;

        private static readonly string[] SsidPrefixes =
        {
            "HomeNetwork", "DIRECT", "Linksys", "Netgear", "TP-LINK",
            "OpenWifi", "CafeWifi", "GuestNet", "AndroidAP", "iPhone"
        };

        private static readonly string[] FileNames =
        {
            "report.pdf", "passwords.txt", "budget.xlsx", "notes.txt",
            "backup.zip", "photo.jpg", "installer.exe", "config.ini",
            "README.md", "secret.key", "data.db", "log.txt"
        };

        private static readonly string[] Folders =
        {
            "documents", "downloads", "desktop", "pictures", "videos"
        };

        public LocationService(LocationConfigSO config)
        {
            _config = config;
        }

        /// <summary>Returns the current location, generating it if needed.</summary>
        public Location GetCurrentLocation()
        {
            return GetOrCreateLocation(_currentLocationId);
        }

        /// <summary>Advances to the next location and returns it.</summary>
        public Location MoveToNextLocation()
        {
            _currentLocationId = _nextLocationId++;
            return GetOrCreateLocation(_currentLocationId);
        }

        private Location GetOrCreateLocation(int id)
        {
            string key = id.ToString();
            if (_locationCache.TryGetValue(key, out Location existing))
                return existing;

            Location loc = GenerateLocation(id);
            _locationCache[key] = loc;
            return loc;
        }

        private Location GenerateLocation(int seed)
        {
            var rng = new System.Random(seed);
            var location = new Location
            {
                Id       = seed.ToString(),
                ConfigId = _config.name,
                IsVisited = false
            };

            int networkCount = rng.Next(_config.MinNetworks, _config.MaxNetworks + 1);
            for (int n = 0; n < networkCount; n++)
            {
                string ssid   = $"{SsidPrefixes[rng.Next(SsidPrefixes.Length)]}_{seed * 100 + n}";
                var secLevel  = PickSecurityLevel(rng);

                var network = new Network
                {
                    Ssid          = ssid,
                    SecurityLevel = secLevel,
                    IsHacked      = false,
                    IsScanned     = false
                };

                int deviceCount = rng.Next(_config.MinDevicesPerNetwork, _config.MaxDevicesPerNetwork + 1);
                for (int d = 0; d < deviceCount; d++)
                {
                    var device = new Device
                    {
                        Ip            = $"192.168.{seed}.{10 + d}",
                        Mac           = GenerateMac(rng),
                        FirewallStatus = FirewallStatus.Active,
                        IsScanned     = false
                    };

                    // Open ports
                    device.OpenPorts.Add(22);
                    if (rng.Next(2) == 0) device.OpenPorts.Add(80);
                    if (rng.Next(2) == 0) device.OpenPorts.Add(443);

                    // Files
                    int fileCount = rng.Next(_config.MinFilesPerDevice, _config.MaxFilesPerDevice + 1);
                    for (int f = 0; f < fileCount; f++)
                    {
                        string folder   = Folders[rng.Next(Folders.Length)];
                        string fileName = FileNames[rng.Next(FileNames.Length)];
                        device.Files.Add(new DeviceFile
                        {
                            Name      = fileName,
                            Path      = $"{folder}/{fileName}",
                            SizeBytes = rng.Next(1024, 50 * 1024 * 1024)
                        });
                    }

                    network.Devices.Add(device);
                }

                location.Networks.Add(network);
            }

            return location;
        }

        private SecurityLevel PickSecurityLevel(System.Random rng)
        {
            float[] dist = _config.SecurityDistribution;
            float total  = 0;
            for (int i = 0; i < dist.Length; i++) total += dist[i];

            float roll = (float)rng.NextDouble() * total;
            float acc  = 0;
            for (int i = 0; i < dist.Length; i++)
            {
                acc += dist[i];
                if (roll <= acc) return (SecurityLevel)i;
            }
            return SecurityLevel.WPA2;
        }

        private static string GenerateMac(System.Random rng)
        {
            return $"{rng.Next(0, 256):X2}:{rng.Next(0, 256):X2}:{rng.Next(0, 256):X2}:" +
                   $"{rng.Next(0, 256):X2}:{rng.Next(0, 256):X2}:{rng.Next(0, 256):X2}";
        }

        // ── Save / Load helpers ──────────────────────────────────────────────

        /// <summary>Restores locations from saved data, setting the next ID correctly.</summary>
        public void RestoreFromSave(List<Models.LocationSaveData> saved, int nextId)
        {
            _locationCache.Clear();
            _nextLocationId = nextId;

            for (int i = 0; i < saved.Count; i++)
            {
                var s   = saved[i];
                var loc = new Location { Id = s.Id, ConfigId = s.ConfigId, IsVisited = s.IsVisited };

                for (int n = 0; n < s.Networks.Count; n++)
                {
                    var ns  = s.Networks[n];
                    var net = new Network
                    {
                        Ssid          = ns.Ssid,
                        SecurityLevel = ns.SecurityLevel,
                        IsHacked      = ns.IsHacked,
                        IsScanned     = ns.IsScanned
                    };

                    for (int d = 0; d < ns.Devices.Count; d++)
                    {
                        var ds  = ns.Devices[d];
                        var dev = new Device
                        {
                            Ip            = ds.Ip,
                            Mac           = ds.Mac,
                            FirewallStatus = ds.FirewallStatus,
                            IsScanned     = ds.IsScanned
                        };
                        dev.OpenPorts.AddRange(ds.OpenPorts);
                        dev.Files.AddRange(ds.Files);
                        if (ds.HasMalware) dev.ActiveMalware = ds.ActiveMalware;
                        net.Devices.Add(dev);
                    }

                    loc.Networks.Add(net);
                }

                _locationCache[s.Id] = loc;
            }

            if (int.TryParse(_locationCache.Count > 0
                    ? _nextLocationId.ToString()
                    : "1", out int parsed))
                _currentLocationId = parsed > 1 ? parsed - 1 : 1;
        }

        /// <summary>Converts current location cache to save data.</summary>
        public List<Models.LocationSaveData> ToSaveData()
        {
            var result = new List<Models.LocationSaveData>();
            foreach (var kv in _locationCache)
            {
                var loc = kv.Value;
                var ls  = new Models.LocationSaveData
                {
                    Id        = loc.Id,
                    ConfigId  = loc.ConfigId,
                    IsVisited = loc.IsVisited
                };

                for (int n = 0; n < loc.Networks.Count; n++)
                {
                    var net = loc.Networks[n];
                    var ns  = new Models.NetworkSaveData
                    {
                        Ssid          = net.Ssid,
                        SecurityLevel = net.SecurityLevel,
                        IsHacked      = net.IsHacked,
                        IsScanned     = net.IsScanned
                    };

                    for (int d = 0; d < net.Devices.Count; d++)
                    {
                        var dev = net.Devices[d];
                        var ds  = new Models.DeviceSaveData
                        {
                            Ip            = dev.Ip,
                            Mac           = dev.Mac,
                            FirewallStatus = dev.FirewallStatus,
                            IsScanned     = dev.IsScanned,
                            HasMalware    = dev.ActiveMalware != null,
                            ActiveMalware = dev.ActiveMalware
                        };
                        ds.OpenPorts.AddRange(dev.OpenPorts);
                        ds.Files.AddRange(dev.Files);
                        ns.Devices.Add(ds);
                    }

                    ls.Networks.Add(ns);
                }

                result.Add(ls);
            }
            return result;
        }

        public int NextLocationId => _nextLocationId;
    }
}
