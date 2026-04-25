using System.Collections.Generic;

namespace HackYourWay.Models
{
    /// <summary>A network-connected machine on a hacked <see cref="Network"/>.</summary>
    [System.Serializable]
    public class Device
    {
        public string Ip;
        public string Mac;
        public FirewallStatus FirewallStatus;

        /// <summary>Open ports on this device.</summary>
        public List<int> OpenPorts = new List<int>();

        /// <summary>Currently installed malware; null if clean. One infection per device.</summary>
        public Malware ActiveMalware;

        /// <summary>True after <c>scan ip</c> or <c>scan mac</c> is executed.</summary>
        public bool IsScanned;

        /// <summary>Files stored on this device, used by ls and copy commands.</summary>
        public List<DeviceFile> Files = new List<DeviceFile>();

        /// <summary>Processing power tier (1–5). Generated at creation; drives miner yield (FR-022).</summary>
        public int CpuTier = 1;

        /// <summary>Network bandwidth tier (1–5). Generated at creation; used in bottleneck calculations (FR-021).</summary>
        public int BandwidthTier = 1;

        /// <summary>GPU tier (0 = none, 1–3 = present). Rare; shown in scan output instead of CPU tier when present.</summary>
        public int GpuTier = 0;

        /// <summary>
        /// Whether malware injection is allowed. Firewall must be disabled,
        /// except on open networks (SecurityLevel.None) where the condition is waived.
        /// </summary>
        public bool CanInject(SecurityLevel parentNetworkSecurity)
        {
            return FirewallStatus == FirewallStatus.Disabled
                   || parentNetworkSecurity == SecurityLevel.None;
        }
    }

    /// <summary>A file stored on a <see cref="Device"/>, accessible via ls and copy.</summary>
    [System.Serializable]
    public class DeviceFile
    {
        /// <summary>Display filename, e.g. "report.pdf".</summary>
        public string Name;

        /// <summary>Full path, e.g. "documents/report.pdf".</summary>
        public string Path;

        /// <summary>File size in bytes, shown in ls output.</summary>
        public long SizeBytes;
    }
}
