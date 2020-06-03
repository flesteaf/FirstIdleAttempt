using Assets.Scripts.Networks.Devices;
using System.Collections.Generic;

namespace Assets.Scripts.Networks
{
    internal class HackableNetwork
    {
        internal string SSID { get; }

        internal List<Device> Devices { get; }
        internal ProtectionType Protection { get; private set; }

        internal NetworkType NetworkSize { get; }

        internal bool WasHacked { get; private set; }

        public HackableNetwork(string ssid, List<Device> devices,
                               ProtectionType protectionType, NetworkType networkSize)
        {
            SSID = ssid;
            Devices = devices;
            Protection = protectionType;
            NetworkSize = networkSize;
            WasHacked = false;
        }

        internal void HackNetwork(ProtectionType crackType)
        {
            if (crackType != Protection)
            {
                return;
            }

            WasHacked = true;
            Protection = ProtectionType.None;
        }

        public override string ToString()
        {
            return $"{SSID} | {Protection}";
        }
    }
}