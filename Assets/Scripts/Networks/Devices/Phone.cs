namespace Assets.Scripts.Networks.Devices
{
    internal class Phone : Device
    {
        public Phone(DeviceIdentification identification) : base(identification)
        {
        }

        internal override bool HasFirewall => false;

        internal override float EnergyLevel => 0.01f;

        internal override float DiskSize => 0.01f;
    }
}