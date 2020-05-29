using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networks.Devices
{
    internal class Phone : Device
    {
        public Phone(DeviceIdentification identification) : base(identification)
        {
        }

        internal override bool HasFirewall => false;

        internal override float EnergyLevel => 0.01f;

        internal override float DiskSize => 0.01f;
    }
}
