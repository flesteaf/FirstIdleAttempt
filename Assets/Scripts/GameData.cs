using Assets.Scripts.Commands;
using Assets.Scripts.Computers;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Softwares;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
    public class GameData
    {
        private float moneyGenerationExponent = 2.912f;

        public List<CommandNames> AvailableSoftware { get; }
        public List<CommandOptions> AvailableSoftwareOptions { get; }
        public float CurrentProduction { get; private set; } = 0;
        public float MoneyAmmount { get; private set; } = 0;
        public List<HackableNetwork> FoundNetworks { get; }
        public Computer Computer { get; }

        public GameData()
        {
            Computer = new InitialComputer();
            AvailableSoftwareOptions = new List<CommandOptions> {
                    CommandOptions.None,
                    CommandOptions.ip,
                    CommandOptions.mac,
                    CommandOptions.network,
                    CommandOptions.miner,
                    CommandOptions.networks,
                    CommandOptions.ips,
                    CommandOptions.money,
                    CommandOptions.computer,
                    CommandOptions.component,
                    CommandOptions.components,
                    CommandOptions.software};

            AvailableSoftware = new List<CommandNames> {
                    CommandNames.help,
                    CommandNames.status,
                    CommandNames.store,
                    CommandNames.buy,
                    CommandNames.scan,
                    CommandNames.inject,
                    CommandNames.show};

            FoundNetworks = new List<HackableNetwork>();
        }

        public void UpdateCurrentProduction(float addValue)
        {
            CurrentProduction += addValue * moneyGenerationExponent;
        }

        public void UpdateAmmount(float value)
        {
            MoneyAmmount += value;
        }

        public void AddProduction()
        {
            MoneyAmmount += CurrentProduction;
        }

        public void AddNetwork(HackableNetwork network)
        {
            network.NetworkHacked += NetworkHacked;
            FoundNetworks.Add(network);
        }

        private void NetworkHacked(HackableNetwork hackedNetwork)
        {
            foreach (var item in hackedNetwork.Devices)
            {
                item.DeviceInfected += DeviceInfected;
            }
        }

        private void DeviceInfected(Device infectedDevice, InfectionType infectionType)
        {
            if (infectionType == InfectionType.Miner)
                UpdateCurrentProduction(infectedDevice.EnergyLevel);
        }

        ~GameData()
        {
            FoundNetworks.RemoveAll(hn => !hn.WasHacked);
            FoundNetworks.SelectMany(n => n.Devices).ToList().ForEach(d => { if (d.IsInfected) d.DeviceInfected -= DeviceInfected; });
            FoundNetworks.ForEach(hn => hn.NetworkHacked -= NetworkHacked);
            FoundNetworks.RemoveAll(hn => true);
        }
    }
}
