namespace Assets.Scripts.Networks.Devices
{
    public sealed class WearableDevice : Device
    {
        public WearableDevice(DeviceIdentification identification) : base(identification)
        {
        }

        public WearableDevice(DeviceIdentification identification, int designatedId) : base(identification, designatedId)
        {
        }

        public override bool HasFirewall => false;

        public override float EnergyLevel => 0.001f;

        public override float DiskSize { get => 0.001f; }

        public override DeviceType Type => DeviceType.Wearable;
    }
}