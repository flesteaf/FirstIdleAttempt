using Assets.Scripts.Networks.Devices;
using System.Collections.Generic;
using System.Diagnostics;
using static Assets.Scripts.HackerDelegates;

namespace Assets.Scripts.Networks
{
    public class HackableNetwork
    {
        public string SSID { get; set; }
        public int DesignatedId { get; set; }
        public List<Device> Devices { get; set; }
        public ProtectionType Protection { get; set; }

        public NetworkType NetworkSize { get; set; }

        public bool WasHacked { get; set; }

        public event NetworkHackedEventHandler NetworkHacked;

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

        public string ToString(bool applyDesignatedId)
        {
            if (applyDesignatedId)
            {
                return $"{DesignatedId} - {this}";
            }

            return ToString();
        }
    }
}