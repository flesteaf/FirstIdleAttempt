namespace Assets.Scripts.Networks.Devices
{
    public class Smartphone : Device
    {
        public Smartphone(DeviceIdentification identification) : base(identification)
        {
        }

        public Smartphone(DeviceIdentification identification, int designatedId) : base(identification, designatedId)
        {
        }

        public override bool HasFirewall => true;

        public override float EnergyLevel => 0.1f;

        public override float DiskSize => 1f;
        public override DeviceType Type => DeviceType.Smartphone;
    }
}