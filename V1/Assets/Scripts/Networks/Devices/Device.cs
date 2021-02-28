using Assets.Scripts.Softwares;
using Newtonsoft.Json;
using static Assets.Scripts.HackerDelegates;

namespace Assets.Scripts.Networks.Devices
{
    public class Device
    {
        private readonly DeviceIdentification identification;
        private bool hasFirewall;

        public int DesignatedId { get; set; }
        public string IP { get => identification.Ip; set => identification.Ip = value; }
        public string MAC { get => identification.Mac; set => identification.Mac = value; }
        public bool FirewallIsActive { get; set; }
        public InfectionType InfectionType { get; set; } = InfectionType.None;
        
        [JsonIgnore]
        public bool CanBeInfected => FirewallIsActive == false;
        [JsonIgnore]
        public bool IsInfected { get => InfectionType != InfectionType.None; }

        public bool HasFirewall 
        {
            get => hasFirewall; 
            set { 
                hasFirewall = value; 
                ActivateFirewall(); 
            } 
        }

        public float EnergyLevel { get; set; }
        public float DiskSize { get; set; }
        public DeviceType Type { get; set; }

        public event DeviceInfectedEventHandler DeviceInfected; 

        public Device()
        {
            identification = new DeviceIdentification();
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

        public string ToString(bool applyDesignatedId)
        {
            if (applyDesignatedId)
            {
                return $"{DesignatedId} - {this}";
            }

            return ToString();
        }
    }
}