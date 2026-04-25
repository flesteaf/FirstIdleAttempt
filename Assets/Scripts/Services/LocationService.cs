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
        // ── Name formula constants (Constitution §I — no magic numbers) ───────
        private const int NameMultiplier = 77;
        private const int NameModulus    = 1000;

        private readonly LocationConfigSO _config;
        private readonly Dictionary<string, Location> _locationCache = new Dictionary<string, Location>();

        // Maintained incrementally; returned by GetAllKnownLocations() with no allocation.
        private readonly List<Location> _knownLocationsSnapshot = new List<Location>();

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

        // ── Location access ──────────────────────────────────────────────────

        /// <summary>Returns the current location, generating it if needed.</summary>
        public virtual Location GetCurrentLocation()
        {
            return GetOrCreateLocation(_currentLocationId);
        }

        /// <summary>Returns true when a current location has been set (cache not empty).</summary>
        public bool HasCurrentLocation() => _locationCache.Count > 0;

        /// <summary>Returns all locations currently known to the player (no allocation).</summary>
        public IReadOnlyList<Location> GetAllKnownLocations() => _knownLocationsSnapshot;

        /// <summary>Returns the name of the current location, or an empty string if none.</summary>
        public string GetCurrentLocationName()
        {
            if (!HasCurrentLocation()) return string.Empty;
            return GetCurrentLocation().Name ?? string.Empty;
        }

        /// <summary>
        /// Sets the current location by name. Returns false when the name is not in the cache.
        /// </summary>
        /// <param name="name">Human-readable location name (e.g. "node_77").</param>
        public bool SetCurrentLocation(string name)
        {
            for (int i = 0; i < _knownLocationsSnapshot.Count; i++)
            {
                if (_knownLocationsSnapshot[i].Name == name)
                {
                    _currentLocationId = int.Parse(_knownLocationsSnapshot[i].Id);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Advances to the next location and returns it.</summary>
        public Location MoveToNextLocation()
        {
            _currentLocationId = _nextLocationId++;
            return GetOrCreateLocation(_currentLocationId);
        }

        // ── Device helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Searches the current location for a device with the given IP across all networks.
        /// Returns null if not found.
        /// </summary>
        public Device FindDevice(string ip)
        {
            if (!HasCurrentLocation()) return null;
            Location loc = GetCurrentLocation();
            for (int n = 0; n < loc.Networks.Count; n++)
            {
                var net = loc.Networks[n];
                for (int d = 0; d < net.Devices.Count; d++)
                {
                    if (net.Devices[d].Ip == ip) return net.Devices[d];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns all scanned devices at the current location (across all networks).
        /// Returns an empty list if no current location or no scanned devices.
        /// </summary>
        public List<Device> GetScannedDevicesAtCurrentLocation()
        {
            var result = new List<Device>();
            if (!HasCurrentLocation()) return result;
            Location loc = GetCurrentLocation();
            for (int n = 0; n < loc.Networks.Count; n++)
            {
                var net = loc.Networks[n];
                for (int d = 0; d < net.Devices.Count; d++)
                {
                    if (net.Devices[d].IsScanned) result.Add(net.Devices[d]);
                }
            }
            return result;
        }

        // ── Forget operations ────────────────────────────────────────────────

        /// <summary>
        /// Removes a network matching <paramref name="ssid"/> from all known locations.
        /// If <paramref name="locationName"/> is specified the search is scoped to that location.
        /// Returns <see cref="ForgetResult.Ambiguous"/> when multiple matches exist and no scope is provided.
        /// </summary>
        /// <param name="ssid">Network SSID to remove.</param>
        /// <param name="locationName">Optional location name to limit the search.</param>
        public ForgetResult ForgetNetwork(string ssid, string locationName = null)
        {
            var matches = new List<(Location loc, Network net)>();

            for (int i = 0; i < _knownLocationsSnapshot.Count; i++)
            {
                var loc = _knownLocationsSnapshot[i];
                if (locationName != null && loc.Name != locationName) continue;

                for (int n = 0; n < loc.Networks.Count; n++)
                {
                    if (loc.Networks[n].Ssid == ssid)
                        matches.Add((loc, loc.Networks[n]));
                }
            }

            if (matches.Count == 0)
                return new ForgetResult(false, $"Network '{ssid}' not found in any known location.");

            if (matches.Count > 1 && locationName == null)
            {
                var names = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++) names[i] = matches[i].loc.Name;
                return ForgetResult.Ambiguous(names);
            }

            // Remove the single (or scoped) match
            int totalIps = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                var (loc, net) = matches[i];
                for (int d = 0; d < net.Devices.Count; d++)
                    if (net.Devices[d].ActiveMalware != null) totalIps++;
                loc.Networks.Remove(net);
            }

            RebuildSnapshot();
            string plural = totalIps == 1 ? "device" : "devices";
            return new ForgetResult(true,
                $"Forgot network '{ssid}' and removed {totalIps} infected {plural}.", totalIps);
        }

        /// <summary>
        /// Removes the device with the given <paramref name="ip"/> from whichever network it belongs to.
        /// </summary>
        public ForgetResult ForgetDevice(string ip)
        {
            for (int i = 0; i < _knownLocationsSnapshot.Count; i++)
            {
                var loc = _knownLocationsSnapshot[i];
                for (int n = 0; n < loc.Networks.Count; n++)
                {
                    var devices = loc.Networks[n].Devices;
                    for (int d = 0; d < devices.Count; d++)
                    {
                        if (devices[d].Ip != ip) continue;
                        int ips = devices[d].ActiveMalware != null ? 1 : 0;
                        devices.RemoveAt(d);
                        RebuildSnapshot();
                        return new ForgetResult(true, $"Forgot device {ip}.", ips);
                    }
                }
            }
            return new ForgetResult(false, $"IP not found in any known location.");
        }

        // ── Internal cache management ────────────────────────────────────────

        private Location GetOrCreateLocation(int id)
        {
            string key = id.ToString();
            if (_locationCache.TryGetValue(key, out Location existing))
                return existing;

            Location loc = GenerateLocation(id);
            _locationCache[key] = loc;
            _knownLocationsSnapshot.Add(loc);
            return loc;
        }

        private void RebuildSnapshot()
        {
            _knownLocationsSnapshot.Clear();
            foreach (var kv in _locationCache)
                _knownLocationsSnapshot.Add(kv.Value);
        }

        // ── Location generation ──────────────────────────────────────────────

        private Location GenerateLocation(int seed)
        {
            var rng = new System.Random(seed);
            var location = new Location
            {
                Id       = seed.ToString(),
                Name     = $"node_{seed * NameMultiplier % NameModulus}",
                ConfigId = _config != null ? _config.name : string.Empty,
                IsVisited = false
            };

            int networkCount = _config != null
                ? rng.Next(_config.MinNetworks, _config.MaxNetworks + 1)
                : rng.Next(2, 5);

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

                int maxDev = _config != null ? _config.MaxDevicesPerNetwork : 4;
                int minDev = _config != null ? _config.MinDevicesPerNetwork : 1;
                int deviceCount = rng.Next(minDev, maxDev + 1);

                for (int d = 0; d < deviceCount; d++)
                {
                    var device = new Device
                    {
                        Ip            = $"192.168.{seed}.{10 + d}",
                        Mac           = GenerateMac(rng),
                        FirewallStatus = FirewallStatus.Active,
                        IsScanned     = false
                    };

                    device.OpenPorts.Add(22);
                    if (rng.Next(2) == 0) device.OpenPorts.Add(80);
                    if (rng.Next(2) == 0) device.OpenPorts.Add(443);

                    int maxFiles = _config != null ? _config.MaxFilesPerDevice : 8;
                    int minFiles = _config != null ? _config.MinFilesPerDevice : 1;
                    int fileCount = rng.Next(minFiles, maxFiles + 1);

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

                    device.CpuTier       = rng.Next(1, 6);
                    device.BandwidthTier = rng.Next(1, 6);

                    network.Devices.Add(device);
                }

                location.Networks.Add(network);
            }

            return location;
        }

        private SecurityLevel PickSecurityLevel(System.Random rng)
        {
            if (_config == null) return SecurityLevel.WPA2;
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
            _knownLocationsSnapshot.Clear();
            _nextLocationId = nextId;

            for (int i = 0; i < saved.Count; i++)
            {
                var s   = saved[i];
                string name = !string.IsNullOrEmpty(s.Name)
                    ? s.Name
                    : $"node_{int.Parse(s.Id) * NameMultiplier % NameModulus}";

                var loc = new Location
                {
                    Id        = s.Id,
                    Name      = name,
                    ConfigId  = s.ConfigId,
                    IsVisited = s.IsVisited
                };

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
                        dev.CpuTier       = ds.CpuTier       > 0 ? ds.CpuTier       : 1;
                        dev.BandwidthTier = ds.BandwidthTier > 0 ? ds.BandwidthTier : 1;
                        net.Devices.Add(dev);
                    }

                    loc.Networks.Add(net);
                }

                _locationCache[s.Id] = loc;
                _knownLocationsSnapshot.Add(loc);
            }
        }

        /// <summary>Converts the current location cache to save data.</summary>
        public List<Models.LocationSaveData> ToSaveData()
        {
            var result = new List<Models.LocationSaveData>();
            foreach (var kv in _locationCache)
            {
                var loc = kv.Value;
                var ls  = new Models.LocationSaveData
                {
                    Id        = loc.Id,
                    Name      = loc.Name,
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
                            ActiveMalware = dev.ActiveMalware,
                            CpuTier       = dev.CpuTier,
                            BandwidthTier = dev.BandwidthTier
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

        /// <summary>Exposes the configuration so consumers (e.g. InjectCommand) can read designer values.</summary>
        public Data.LocationConfigSO Config => _config;
    }
}
