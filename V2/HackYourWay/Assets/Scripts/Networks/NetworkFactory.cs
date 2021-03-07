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

        private readonly List<INetworkGeneration> networkData;
        private int lastNetworkId;

        public NetworkFactory()
        {
            deviceFactory = new DeviceFactory();
            random = new Random();
            networkData = new List<INetworkGeneration>
            {
                new HomeNetworkGeneration(),
                new SmallNetworkGeneration(),
                new SmallOfficeNetworkGeneration()
            };
        }

        public HackableNetwork GetRandomNetwork(NetworkType maxType, bool applyDesignatedId = false)
        {
            int networkType = random.Next(0, (int)maxType);
            NetworkType networkTypeToGenerate = (NetworkType)Enum.Parse(typeof(NetworkType), networkType.ToString());

            return GetNetwork(networkTypeToGenerate, applyDesignatedId);
        }

        private HackableNetwork GetNetwork(NetworkType netType, bool applyDesignatedId)
        {
            List<DeviceIdentification> deviceIdentifications = new List<DeviceIdentification>();
            INetworkGeneration generationData = networkData.First(x => x.Type == netType);
            for (int i = 0; i < generationData.NoOfDevices; i++)
            {
                deviceIdentifications.Add(GetDeviceIdetification());
            }

            string ssid = generationData.GetSSID();
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

        internal NetworkType GetRandomNetworkType()
        {
            Array networkTypes = Enum.GetValues(typeof(NetworkType));
            return (NetworkType)networkTypes.GetValue(random.Next(networkTypes.Length));
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