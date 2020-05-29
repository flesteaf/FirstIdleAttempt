namespace Assets.Scripts.Networks.Devices
{
    internal abstract class Device
    {
        private DeviceIdentification identification;

        internal string IP { get => identification.Ip; }
        internal string MAC { get => identification.Mac; }

        internal abstract bool HasFirewall { get; }
        internal abstract float EnergyLevel { get; }
        internal abstract float DiskSize { get; }

        public Device(DeviceIdentification identification)
        {
            this.identification = identification;
        }
    }
}
