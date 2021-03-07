using HackingYourWay.Assets.Scripts.Networks.Generation;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Networks
{
    internal class HomeNetworkGeneration : INetworkGeneration
    {
        private readonly List<string> homeSsids = new List<string> { "MyHome", "PersonalSpace", "SmoothCriminal", "Alphabet", "Home" };

        public NetworkType Type => NetworkType.Home;

        public int NoOfDevices => 5;

        public ProtectionType Protection => ProtectionType.None;

        public string GetSSID()
        {
            return homeSsids[new Random().Next(0, homeSsids.Count - 1)];
        }
    }
}