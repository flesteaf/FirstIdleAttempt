using HackingYourWay.Assets.Scripts.Networks.Generation;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Networks
{
    internal class SmallNetworkGeneration : INetworkGeneration
    {
        private readonly List<string> smallSsids = new List<string> { "Ghost", "IBD", "Sockass" };

        public NetworkType Type => NetworkType.Small;
         
        public int NoOfDevices => 8;
         
        public ProtectionType Protection => ProtectionType.WEP;

        public string GetSSID()
        {
            return smallSsids[new Random().Next(0, smallSsids.Count - 1)];
        }
    }
}