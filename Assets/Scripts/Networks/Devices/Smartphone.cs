namespace Assets.Scripts.Networks.Devices
{
    public class Smartphone : Device
    {
        public Smartphone()
        {
            HasFirewall = true;
            EnergyLevel = 0.1f;
            DiskSize = 1f;
            Type = DeviceType.Smartphone;
        }
    }
}