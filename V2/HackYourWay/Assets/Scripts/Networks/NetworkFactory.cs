using Assets.Scripts.Networks.Devices;
using HackingYourWay.Assets.Scripts.Networks.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Networks
{
    internal class NetworkFactory
    {
        private readonly DeviceFactory deviceFactory;
        private readonly Random random;

        private readonly List<NetworkGeneration> networkData;
        private int lastNetworkId;
        private List<string> generatedSsids = new List<string>();

        public NetworkFactory()
        {
            deviceFactory = new DeviceFactory();
            random = new Random();
            networkData = new List<NetworkGeneration>
            {
                new HomeNetworkGeneration(),
                new SmallNetworkGeneration(),
                new SmallOfficeNetworkGeneration()
            };
        }

        public HackableNetwork GetRandomNetwork(bool applyDesignatedId)
        {
            int networkType = random.Next(0, 91);
            NetworkType networkTypeToGenerate = NetworkType.Home;

            //revert to switch when unity support C# 7.0
            if (networkType >= 0 && networkType <= 40)
                networkTypeToGenerate = NetworkType.Home;

            if (networkType >= 41 && networkType <= 70) 
                    networkTypeToGenerate = NetworkType.Small; 

            if (networkType >= 71 && networkType <= 90)
                    networkTypeToGenerate = NetworkType.SmallOffice; 

            return GetNetwork(networkTypeToGenerate, applyDesignatedId);
        }

        private HackableNetwork GetNetwork(NetworkType netType, bool applyDesignatedId)
        {
            List<DeviceIdentification> deviceIdentifications = new List<DeviceIdentification>();
            NetworkGeneration generationData = networkData.First(x => x.Type == netType);
            for (int i = 0; i < generationData.NoOfDevices; i++)
            {
                deviceIdentifications.Add(GetDeviceIdetification());
            }

            string ssid = generationData.GetSSID(generatedSsids);
            List<Device> devices = deviceFactory.GetDevices(deviceIdentifications, applyDesignatedId);

            return new HackableNetwork
            {
                SSID = ssid,
                Devices = devices,
                Protection = generationData.Protection,
                NetworkSize = netType,
                DesignatedId = applyDesignatedId ? lastNetworkId++ : 0
            };
        }

        private DeviceIdentification GetDeviceIdetification()
        {
            string ip = GenerateIp();
            string mac = GenerateMac();

            return new DeviceIdentification { Ip = ip, Mac = mac };
        }

        private string GenerateMac()
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                builder.Append(random.Next(0, 255).ToString("X2") + ":");
            }
            builder.Append(random.Next(0, 255).ToString("X2"));

            return builder.ToString();
        }

        private string GenerateIp()
        {
            return $"{random.Next(1, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}";
        }
    }
}