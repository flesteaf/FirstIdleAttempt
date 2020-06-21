using Assets.Scripts.Softwares;
using static Assets.Scripts.HackingDelegates;

namespace Assets.Scripts.Networks.Devices
{
    public abstract class Device
    {
        private readonly DeviceIdentification identification;

        public string IP { get => identification.Ip; }
        public string MAC { get => identification.Mac; }
        public bool FirewallIsActive { get; private set; }
        public bool CanBeInfected => FirewallIsActive == false;
        public bool IsInfected { get => InfectionType != InfectionType.None; }
        public InfectionType InfectionType { get; private set; }

        public abstract bool HasFirewall { get; }
        public abstract float EnergyLevel { get; }
        public abstract float DiskSize { get; }

        public event DeviceInfectedEventHandler DeviceInfected; 

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

        internal void Infect(InfectionType type)
        {
            InfectionType = type;
            DeviceInfected?.Invoke(this, type);
        }

        public override string ToString()
        {
            return $"{IP} | {MAC}";
        }
    }
}