namespace Assets.Scripts.Networks.Devices
{
    internal abstract class Device
    {
        private readonly DeviceIdentification identification;

        internal string IP { get => identification.Ip; }
        internal string MAC { get => identification.Mac; }

        internal abstract bool HasFirewall { get; }
        internal bool FirewallIsActive { get; private set; }
        internal abstract float EnergyLevel { get; }
        internal abstract float DiskSize { get; }

        public Device(DeviceIdentification identification)
        {
            this.identification = identification;
            ActivateFirewall();
        }

        internal void DeactivateFirewall()
        {
            if (HasFirewall)
            {
                FirewallIsActive = false;
            }
        }

        internal void ActivateFirewall()
        {
            if (HasFirewall)
            {
                FirewallIsActive = false;
            }
        }

        public override string ToString()
        {
            return $"{IP} | {MAC}";
        }
    }
}