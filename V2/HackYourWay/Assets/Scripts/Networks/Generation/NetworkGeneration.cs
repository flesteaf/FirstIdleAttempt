using Assets.Scripts.Networks;
using System.Collections.Generic;

namespace HackingYourWay.Assets.Scripts.Networks.Generation
{
    internal abstract class NetworkGeneration
    {
        public abstract NetworkType Type { get; }
        public abstract int NoOfDevices { get; }
        public abstract ProtectionType Protection { get; }
        public string GetSSID(List<string> generatedSsids)
        {
            string ssid;
            do
            {
                ssid = GetSSID();
            } while (generatedSsids.Contains(ssid));

            generatedSsids.Add(ssid);

            return ssid;
        }

        protected abstract string GetSSID();
    }
}