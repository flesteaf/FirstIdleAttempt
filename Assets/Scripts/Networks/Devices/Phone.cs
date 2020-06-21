namespace Assets.Scripts.Networks.Devices
{
    public class Phone : Device
    {
        public Phone(DeviceIdentification identification) : base(identification)
        {
        }

        public override bool HasFirewall => false;
        public override float EnergyLevel => 0.01f;
        public override float DiskSize => 0.01f;
    }
}