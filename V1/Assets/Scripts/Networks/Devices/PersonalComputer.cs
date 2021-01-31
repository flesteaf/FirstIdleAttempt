namespace Assets.Scripts.Networks.Devices
{
    public class PersonalComputer : Device
    {
        public PersonalComputer()
        {
            HasFirewall = true;
            EnergyLevel = 1f;
            DiskSize = 5f;
            Type = DeviceType.PersonalComputer;
        }
    }
}