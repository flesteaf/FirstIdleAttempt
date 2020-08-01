using Assets.Scripts.Networks.Devices;
using System.Collections.Generic;
using static Assets.Scripts.HackerDelegates;

namespace Assets.Scripts.Networks
{
    public class HackableNetwork
    {
        public string SSID { get; }

        public List<Device> Devices { get; }
        public ProtectionType Protection { get; private set; }

        public NetworkType NetworkSize { get; }

        public bool WasHacked { get; private set; }

        public event NetworkHackedEventHandler NetworkHacked;

        public HackableNetwork(string ssid, List<Device> devices,
                               ProtectionType protectionType, NetworkType networkSize)
        {
            SSID = ssid;
            Devices = devices;
            Protection = protectionType;
            NetworkSize = networkSize;
            WasHacked = false;
        }

        public void HackNetwork(ProtectionType crackType)
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