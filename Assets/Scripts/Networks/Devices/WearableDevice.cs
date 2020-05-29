using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networks.Devices
{
    internal sealed class WearableDevice : Device
    {
        public WearableDevice(DeviceIdentification identification) : base(identification)
        {
        }

        internal override bool HasFirewall => false;

        internal override float EnergyLevel => 0.001f;

        internal override float DiskSize { get => 0.001f; }
    }
}
