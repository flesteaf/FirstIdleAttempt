using Assets.Scripts.Networks.Devices;
using System.Collections.Generic;

namespace Assets.Scripts.Networks
{
    internal class HackableNetwork
    {
        internal string SSID { get; }

        internal List<Device> Devices { get; }
        internal ProtectionType Protection { get; }

        internal NetworkType NetworkSize { get; }

        public HackableNetwork(string ssid, List<Device> devices,
                               ProtectionType protectionType, NetworkType networkSize)
        {
            SSID = ssid;
            Devices = devices;
            Protection = protectionType;
            NetworkSize = networkSize;
        }
    }
}