using System.Collections.Generic;

namespace HackYourWay.Models
{
    /// <summary>A discoverable Wi-Fi access point within a <see cref="Location"/>.</summary>
    [System.Serializable]
    public class Network
    {
        /// <summary>Display name; unique within its parent location.</summary>
        public string Ssid;

        /// <summary>Determines which crack tool is required.</summary>
        public SecurityLevel SecurityLevel;

        /// <summary>Connected devices, populated after <c>scan network</c>.</summary>
        public List<Device> Devices = new List<Device>();

        /// <summary>True after a successful crack command.</summary>
        public bool IsHacked;

        /// <summary>True after <c>scan network {SSID}</c> is executed.</summary>
        public bool IsScanned;
    }
}
