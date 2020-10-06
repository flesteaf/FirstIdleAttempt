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
            Type device = Devices[random];

            if (applyDesignatedId)
            {
                return (Device)Activator.CreateInstance(device, identification, lastDeviceId++);
            }

            return (Device)Activator.CreateInstance(device, identification);
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
                return new PersonalComputer(identification, lastDeviceId++);
            }

            return new PersonalComputer(identification);
        }

        public Device GetPhone(DeviceIdentification identification, bool applyDesignatedId = false)
        {
            if (applyDesignatedId)
            {
                return new Phone(identification, lastDeviceId++);
            }

            return new Phone(identification);
        }

        public Device GetSmartphone(DeviceIdentification identification, bool applyDesignatedId = false)
        {
            if (applyDesignatedId)
            {
                return new Smartphone(identification, lastDeviceId++);
            }

            return new Smartphone(identification);
        }

        public Device GetWearableDevice(DeviceIdentification identification, bool applyDesignatedId = false)
        {
            if (applyDesignatedId)
            {
                return new WearableDevice(identification, lastDeviceId++);
            }

            return new WearableDevice(identification);
        }
    }
}