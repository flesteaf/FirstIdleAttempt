using HackingYourWay.Assets.Scripts.Networks.Generation;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Networks
{
    internal class SmallOfficeNetworkGeneration : INetworkGeneration
    {
        private readonly List<string> officeSsids = new List<string> { "OnlyPrinting", "Decisions", "The oners" };

        public NetworkType Type => NetworkType.SmallOffice;
         
        public int NoOfDevices => 15;
         
        public ProtectionType Protection => ProtectionType.WPA;

        public string GetSSID()
        {
            return officeSsids[new Random().Next(0, officeSsids.Count - 1)];
        }
    }
}