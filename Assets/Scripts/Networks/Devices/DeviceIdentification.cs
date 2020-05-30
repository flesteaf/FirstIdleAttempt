namespace Assets.Scripts.Networks.Devices
{
    internal class DeviceIdentification
    {
        public string Ip { get; }
        public string Mac { get; }

        public DeviceIdentification(string ip, string mac)
        {
            Ip = ip;
            Mac = mac;
        }
    }
}