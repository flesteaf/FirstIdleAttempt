using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Networks.Devices
{
    internal class DeviceFactory
    {
        private readonly List<Type> Devices = new List<Type>();

        public DeviceFactory()
        {
            Devices.Add(typeof(PersonalComputer));
            Devices.Add(typeof(Phone));
            Devices.Add(typeof(Smartphone));
            Devices.Add(typeof(WearableDevice));
        }

        public Device GetRandomDevice(DeviceIdentification identification)
        {
            int random = new Random().Next(0, Devices.Count - 1);
            Type device = Devices[random];

            return (Device)Activator.CreateInstance(device, identification);
        }

        public List<Device> GetDevices(List<DeviceIdentification> identifications)
        {
            List<Device> currentDevices = identifications.Select(x => GetRandomDevice(x)).ToList();

            return currentDevices;
        }

        public Device GetPersonalComputer(DeviceIdentification identification)
        {
            return new PersonalComputer(identification);
        }

        public Device GetPhone(DeviceIdentification identification)
        {
            return new Phone(identification);
        }

        public Device GetSmartphone(DeviceIdentification identification)
        {
            return new Smartphone(identification);
        }

        public Device GetWearableDevice(DeviceIdentification identification)
        {
            return new WearableDevice(identification);
        }
    }
}
