namespace Assets.Scripts.Networks.Devices
{
    internal abstract class Device
    {
        internal string IP { get; }
        internal string MAC { get; }

        internal abstract bool HasFirewall { get; }
        internal abstract float EnergyLevel { get; }
        internal abstract float DiskSize { get; }

        public Device(string ip, string mac)
        {
            IP = ip;
            MAC = mac;
        }
    }
}
