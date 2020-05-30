namespace Assets.Scripts.Networks.Devices
{
    internal class PersonalComputer : Device
    {
        public PersonalComputer(DeviceIdentification identification) : base(identification)
        {
        }

        internal override bool HasFirewall => true;

        internal override float EnergyLevel => 1f;

        internal override float DiskSize => 5f;
    }
}