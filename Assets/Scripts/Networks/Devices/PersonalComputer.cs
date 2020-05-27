using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networks.Devices
{
    internal class PersonalComputer : Device
    {
        public PersonalComputer(string ip, string mac) : base(ip, mac)
        {
        }

        internal override bool HasFirewall => true;

        internal override float EnergyLevel => 1f;

        internal override float DiskSize => 5f;
    }
}
