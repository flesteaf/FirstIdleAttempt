using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Softwares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class HackingDelegates
    {
        public delegate void NetworkHackedEventHandler(HackableNetwork hackedNetwork);
        public delegate void DeviceInfectedEventHandler(Device infectedDevice, InfectionType infectionType);
    }
}
