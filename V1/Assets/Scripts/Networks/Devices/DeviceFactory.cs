using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Networks.Devices
{
    internal class DeviceFactory
    {
        private readonly List<Type> Devices = new List<Type>();
        private int lastDeviceId;

        public DeviceFactory()
        {
            Devices.Add(typeof(PersonalComputer));
            Devices.Add(typeof(Phone));
            Devices.Add(typeof(Smartphone));
            Devices.Add(typeof(WearableDevice));
        }

        public Device GetRandomDevice(DeviceIdentification identification, bool applyDesignatedId)
        {
            int random = new Random().Next(0, Devices.Count - 1);
            Type deviceType = Devices[random];
            Device device;

            if (applyDesignatedId)
            {
                device = (Device)Activator.CreateInstance(deviceType);
                device.IP = identification.Ip;
                device.MAC = identification.Mac;
                device.DesignatedId = lastDeviceId++;
                return device;
            }

            device = (Device)Activator.CreateInstance(deviceType);
            device.IP = identification.Ip;
            device.MAC = identification.Mac;

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