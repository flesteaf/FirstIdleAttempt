using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Networks.Devices
{
    internal class DeviceFactory
    {
        private readonly List<Type> Devices = new List<Type>();
        private int lastDeviceId;
        private readonly System.Random random = new System.Random();

        public DeviceFactory()
        {
            Devices.Add(typeof(Phone));
            Devices.Add(typeof(WearableDevice));
            Devices.Add(typeof(Smartphone));
            Devices.Add(typeof(PersonalComputer));
        }

        public Device GetRandomDevice(DeviceIdentification identification, bool applyDesignatedId)
        {
            int randomNo = random.Next(0, 101);

            if (randomNo >= 0 && randomNo <= 40)
                randomNo = 0;

            if (randomNo >= 41 && randomNo <= 70)
                randomNo = 1;

            if (randomNo >= 71 && randomNo <= 90)
                randomNo = 2;

            if (randomNo >= 91 && randomNo <= 100)
                randomNo = 3;

            Type deviceType = Devices[randomNo];
            Device device;
            device = (Device)Activator.CreateInstance(deviceType);
            device.IP = identification.Ip;
            device.MAC = identification.Mac;

            if (applyDesignatedId)
            {
                device.DesignatedId = lastDeviceId++;
            }

            return device;
        }

        public List<Device> GetDevices(List<DeviceIdentification> identifications, bool applyDesignatedId)
        {
            List<Device> currentDevices = identifications.Select(x => GetRandomDevice(x, applyDesignatedId)).ToList();

            return currentDevices;
        }

        public Device GetPersonalComputer(DeviceIdentification identification, bool applyDesignatedId = false)
        {
            if (applyDesignatedId)
            {
                return new PersonalComputer { IP = identification.Ip, MAC = identification.Mac, DesignatedId = lastDeviceId++ };
            }

            return new PersonalComputer { IP = identification.Ip, MAC = identification.Mac };
        }

        public Device GetPhone(DeviceIdentification identification, bool applyDesignatedId = false)
        {
            if (applyDesignatedId)
            {
                return new Phone { IP = identification.Ip, MAC = identification.Mac, DesignatedId = lastDeviceId++ };
            }

            return new Phone { IP = identification.Ip, MAC = identification.Mac };
        }

        public Device GetSmartphone(DeviceIdentification identification, bool applyDesignatedId = false)
        {
            if (applyDesignatedId)
            {
                return new Smartphone { IP = identification.Ip, MAC = identification.Mac, DesignatedId = lastDeviceId++ };
            }

            return new Smartphone { IP = identification.Ip, MAC = identification.Mac };
        }

        public Device GetWearableDevice(DeviceIdentification identification, bool applyDesignatedId = false)
        {
            if (applyDesignatedId)
            {
                return new WearableDevice { IP = identification.Ip, MAC = identification.Mac, DesignatedId = lastDeviceId++ };
            }

            return new WearableDevice { IP = identification.Ip, MAC = identification.Mac };
        }
    }
}