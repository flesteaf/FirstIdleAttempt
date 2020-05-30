using Assets.Scripts.Networks.Devices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Networks
{
    internal class NetworkFactory
    {
        private readonly DeviceFactory deviceFactory;
        private readonly Random random;
        private readonly List<string> homeSsids = new List<string> { "MyHome", "PersonalSpace", "SmoothCriminal" };
        private readonly List<string> smallSsids = new List<string> { "Ghost", "IBD", "Sockass" };
        private readonly int homeSsidsCount;
        private readonly int smallSsidsCount;

        public NetworkFactory()
        {
            deviceFactory = new DeviceFactory();
            random = new Random();

            homeSsidsCount = homeSsids.Count;
            smallSsidsCount = smallSsids.Count;
        }

        public HackableNetwork GetHomeNetwork()
        {
            int noOfDevices = 4;
            List<DeviceIdentification> deviceIdentifications = new List<DeviceIdentification>();
            for (int i = 0; i < noOfDevices; i++)
            {
                deviceIdentifications.Add(GetDeviceIdetification());
            }

            string ssid = GetHomeSSID();
            List<Device> devices = deviceFactory.GetDevices(deviceIdentifications);

            return new HackableNetwork(ssid, devices, ProtectionType.None, NetworkType.Home);
        }

        public HackableNetwork GetSmallNetwork()
        {
            int noOfDevices = 6;
            List<DeviceIdentification> deviceIdentifications = new List<DeviceIdentification>();
            for (int i = 0; i < noOfDevices; i++)
            {
                deviceIdentifications.Add(GetDeviceIdetification());
            }

            string ssid = GetSmallSSID();
            List<Device> devices = deviceFactory.GetDevices(deviceIdentifications);

            return new HackableNetwork(ssid, devices, ProtectionType.None, NetworkType.Small);
        }

        private DeviceIdentification GetDeviceIdetification()
        {
            string ip = GenerateIp();
            string mac = GenerateMac();

            return new DeviceIdentification(ip, mac);
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

        private string GetHomeSSID()
        {
            return homeSsids[random.Next(0, homeSsidsCount - 1)];
        }

        private string GetSmallSSID()
        {
            return smallSsids[random.Next(0, smallSsidsCount - 1)];
        }
    }
}