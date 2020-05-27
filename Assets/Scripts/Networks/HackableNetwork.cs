using Assets.Scripts.Networks.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networks
{
    internal abstract class HackableNetwork
    {
        internal string SSID { get; }

        internal abstract List<Device> Devices { get; }
        internal abstract ProtectionType Protection { get; }

        public HackableNetwork(string ssid)
        {
            SSID = ssid;
        }
    }
}
