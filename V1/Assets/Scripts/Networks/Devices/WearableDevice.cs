namespace Assets.Scripts.Networks.Devices
{
    public sealed class WearableDevice : Device
    {
        public WearableDevice()
        {
            HasFirewall = false;
            EnergyLevel = 0.001f;
            DiskSize = 0.001f;
            Type = DeviceType.Wearable;
        }
    }
}