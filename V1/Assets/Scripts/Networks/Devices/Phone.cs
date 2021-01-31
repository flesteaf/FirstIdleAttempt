namespace Assets.Scripts.Networks.Devices
{
    public class Phone : Device
    {
        public Phone()
        {
            HasFirewall = false;
            EnergyLevel = 0.01f;
            DiskSize = 0.01f;
            Type = DeviceType.Phone;
        }
    }
}