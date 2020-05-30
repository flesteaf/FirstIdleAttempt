namespace Assets.Scripts.Networks.Devices
{
    internal class Smartphone : Device
    {
        public Smartphone(DeviceIdentification identification) : base(identification)
        {
        }

        internal override bool HasFirewall => true;

        internal override float EnergyLevel => 0.1f;

        internal override float DiskSize => 1f;
    }
}