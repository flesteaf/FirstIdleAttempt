using Assets.Scripts.Networks.Devices;
using System.Collections.Generic;
using static Assets.Scripts.HackingDelegates;

namespace Assets.Scripts.Networks
{
    internal class HackableNetwork
    {
        internal string SSID { get; }

        internal List<Device> Devices { get; }
        internal ProtectionType Protection { get; private set; }

        internal NetworkType NetworkSize { get; }

        internal bool WasHacked { get; private set; }

        internal event NetworkHackedEventHandler NetworkHacked;

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
            NetworkHacked?.Invoke(this);
        }

        public override string ToString()
        {
            return $"{SSID} | {Protection}";
        }
    }
}