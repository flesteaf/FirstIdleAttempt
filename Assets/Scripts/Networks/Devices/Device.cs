using Assets.Scripts.Software;

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

        internal bool IsInfected { get => InfectionType != InfectionType.None; }
        internal InfectionType InfectionType { get; private set; }

        public Device(DeviceIdentification identification)
        {
            this.identification = identification;
            FirewallIsActive = false;
            InfectionType = InfectionType.None;
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
                FirewallIsActive = true;
            }
        }

        internal bool CanInfect()
        {
            return FirewallIsActive == false;
        }

        internal void Infect(InfectionType type)
        {
            InfectionType = type;
        }

        public override string ToString()
        {
            return $"{IP} | {MAC}";
        }
    }
}