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
        private readonly List<string> homeSsids = new List<string> { "MyHome", "PersonalSpace", "SmoothCriminal", "Alphabet", "Home" };
        private readonly List<string> smallSsids = new List<string> { "Ghost", "IBD", "Sockass" };
        private readonly List<string> mediumSsids = new List<string> { "OnlyPrinting", "Decisions", "The oners" };
        private readonly int homeSsidsCount;
        private readonly int smallSsidsCount;
        private readonly int mediumSsidsCount;
        private int lastNetworkId;

        public NetworkFactory()
        {
            deviceFactory = new DeviceFactory();
            random = new Random();

            homeSsidsCount = homeSsids.Count;
            smallSsidsCount = smallSsids.Count;
            mediumSsidsCount = mediumSsids.Count;
        }

        public HackableNetwork GetRandomNetwork(NetworkType maxType, bool applyDesignatedId = false)
        {
            int networkType = random.Next(0, (int)maxType);
            NetworkType networkTypeToGenerate = (NetworkType)Enum.Parse(typeof(NetworkType), networkType.ToString());

            switch (networkTypeToGenerate)
            {
                case NetworkType.Home:
                    return GetHomeNetwork(applyDesignatedId);

                case NetworkType.Small:
                    return GetSmallNetwork(applyDesignatedId);

                case NetworkType.Medium:
                    return GetMediumNetwork(applyDesignatedId);

                case NetworkType.Huge:
                    return GetHugeNetwork(applyDesignatedId);

                default:
                    return null;
            }
        }

        public HackableNetwork GetHomeNetwork(bool applyDesignatedId)
        {
            int noOfDevices = 4;
            List<DeviceIdentification> deviceIdentifications = new List<DeviceIdentification>();
            for (int i = 0; i < noOfDevices; i++)
            {
                deviceIdentifications.Add(GetDeviceIdetification());
            }

            string ssid = GetHomeSSID();
            List<Device> devices = deviceFactory.GetDevices(deviceIdentifications, applyDesignatedId);

            if (applyDesignatedId)
            {
                return new HackableNetwork
                {
                    SSID = ssid,
                    Devices = devices,
                    Protection = ProtectionType.None,
                    NetworkSize = NetworkType.Home,
                    DesignatedId = lastNetworkId++
                };
            }

            return new HackableNetwork
            {
                SSID = ssid,
                Devices = devices,
                Protection = ProtectionType.None,
                NetworkSize = NetworkType.Home
            };
        }

        public HackableNetwork GetSmallNetwork(bool applyDesignatedId)
        {
            int noOfDevices = 8;
            List<DeviceIdentification> deviceIdentifications = new List<DeviceIdentification>();
            for (int i = 0; i < noOfDevices; i++)
            {
                deviceIdentifications.Add(GetDeviceIdetification());
            }

            string ssid = GetSmallSSID();
            List<Device> devices = deviceFactory.GetDevices(deviceIdentifications, applyDesignatedId);

            if (applyDesignatedId)
            {
                return new HackableNetwork
                {
                    SSID = ssid,
                    Devices = devices,
                    Protection = ProtectionType.WEP,
                    NetworkSize = NetworkType.Small,
                    DesignatedId = lastNetworkId++
                };
            }

            return new HackableNetwork
            {
                SSID = ssid,
                Devices = devices,
                Protection = ProtectionType.WEP,
                NetworkSize = NetworkType.Small
            };
        }

        public HackableNetwork GetMediumNetwork(bool applyDesignatedId)
        {
            int noOfDevices = 18;
            List<DeviceIdentification> deviceIdentifications = new List<DeviceIdentification>();
            for (int i = 0; i < noOfDevices; i++)
            {
                deviceIdentifications.Add(GetDeviceIdetification());
            }

            string ssid = GetMediumSSID();
            List<Device> devices = deviceFactory.GetDevices(deviceIdentifications, applyDesignatedId);

            if (applyDesignatedId)
            {
                return new HackableNetwork
                {
                    SSID = ssid,
                    Devices = devices,
                    Protection = ProtectionType.WPA,
                    NetworkSize = NetworkType.Small,
                    DesignatedId = lastNetworkId++
                };
            }

            return new HackableNetwork
            {
                SSID = ssid,
                Devices = devices,
                Protection = ProtectionType.WPA,
                NetworkSize = NetworkType.Small
            };
        }

        public HackableNetwork GetHugeNetwork(bool applyDesignatedId)
        {
            //TODO: implement
            throw new NotImplementedException();
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

        private string GetHomeSSID()
        {
            return homeSsids[random.Next(0, homeSsidsCount - 1)];
        }

        private string GetSmallSSID()
        {
            return smallSsids[random.Next(0, smallSsidsCount - 1)];
        }

        private string GetMediumSSID()
        {
            return mediumSsids[random.Next(0, mediumSsidsCount - 1)];
        }
    }
}