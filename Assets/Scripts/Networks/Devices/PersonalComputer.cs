﻿namespace Assets.Scripts.Networks.Devices
{
    public class PersonalComputer : Device
    {
        public PersonalComputer(DeviceIdentification identification) : base(identification)
        {
        }

        public PersonalComputer(DeviceIdentification identification, int designatedId) : base(identification, designatedId)
        {
        }

        public override bool HasFirewall => true;
        public override float EnergyLevel => 1f;
        public override float DiskSize => 5f;

        public override DeviceType Type => DeviceType.PersonalComputer;
    }
}