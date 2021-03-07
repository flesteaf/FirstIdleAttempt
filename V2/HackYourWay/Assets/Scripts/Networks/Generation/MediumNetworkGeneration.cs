using Assets.Scripts.Networks;
using System;
using System.Collections.Generic;

namespace HackingYourWay.Assets.Scripts.Networks.Generation
{
    internal class MediumNetworkGeneration : INetworkGeneration
    {
        private readonly List<string> mediumSsids = new List<string> { "TheHome", "Hacker", "DND", "DontBotherMe", "IDK" };
        
        public NetworkType Type => NetworkType.Medium;

        public int NoOfDevices => 20;

        public ProtectionType Protection => ProtectionType.WPA;

        public string GetSSID()
        {
            return mediumSsids[new Random().Next(mediumSsids.Count)];
        }
    }
}
