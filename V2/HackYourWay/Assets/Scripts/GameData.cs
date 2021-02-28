using Assets.Scripts.Commands;
using Assets.Scripts.Computers;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using static Assets.Scripts.HackerDelegates;
using Random = System.Random;

namespace Assets.Scripts
{
    public class GameData : IGameData
    {
        private readonly float moneyGenerationExponent = 2.912f;
        private readonly NetworkFactory networkFactory;
        private readonly Random random;
        private static bool commandUnderExecution;

        public List<CommandNames> AvailableSoftware { get; set; }
        public List<CommandOptions> AvailableSoftwareOptions { get; set; }
        public float CurrentProduction { get; set; } = 0;
        public float MoneyAmmount { get; set; } = 0;
        public List<HackableNetwork> FoundNetworks { get; set; }
        public Computer Computer { get; set; }
        public GameStore Store { get; set; }

        public bool ApplyDesignatedId => AvailableSoftware.Contains(CommandNames.extract);

        public bool CommandUnderExecution { get => commandUnderExecution; set => commandUnderExecution = value; }

        public event SendMessageEventHandler MessageSender;
        public event ClearConsoleEventHandler ClearHandler;

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
                    CommandNames.show,
                    CommandNames.clear};

            FoundNetworks = new List<HackableNetwork>();
            networkFactory = new NetworkFactory();
            random = new Random();
            commandUnderExecution = false;
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

        #region Networks

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

        public void RefreshNetworks()
        {
            FoundNetworks.RemoveAll(n => !n.WasHacked);

            int noOfNetworksToDiscover = random.Next(1, 5);
            for (int i = 0; i < noOfNetworksToDiscover; i++)
            {
                HackableNetwork item;
                if (ApplyDesignatedId)
                {
                    item = networkFactory.GetRandomNetwork(NetworkType.Medium, true);
                }
                else
                {
                    item = networkFactory.GetRandomNetwork(NetworkType.Medium);
                }

                AddNetwork(item);
                SendMessage(item.ToString(ApplyDesignatedId), MessageType.Info);
            }
        }

        public IEnumerable<Device> GetAllHackedDevices()
        {
            return FoundNetworks.SelectMany(n => n.Devices).Where(d => d.IsInfected);
        }

        public Device GetDeviceByIp(string ip)
        {
            Device[] devices = FoundNetworks.SelectMany(n => n.Devices).ToArray();
            Device device = Array.Find(devices, d => d.IP == ip);

            if (device == null && ApplyDesignatedId) 
            {
                if (int.TryParse(ip.Substring(2), out int desiredId))
                {
                    device = Array.Find(devices, d => d.DesignatedId == desiredId);
                }
            }

            return device;
        }

        public HackableNetwork GetNetworkBySSID(string ssid)
        {
            HackableNetwork network = FoundNetworks.Find(n => n.SSID.Equals(ssid, StringComparison.OrdinalIgnoreCase));
            if (network == null && ApplyDesignatedId)
            {
                if (int.TryParse(ssid.Substring(3), out int desiredId))
                {
                    network = FoundNetworks.Find(n => n.DesignatedId == desiredId);
                }
            }

            if (network != null && network.Protection == ProtectionType.None && !network.WasHacked)
            {
                network.HackNetwork(ProtectionType.None);
            }

            return network;
        }

        public Device GetDeviceByMac(string mac)
        {
            Device[] devices = FoundNetworks.SelectMany(n => n.Devices).ToArray();
            Device device = Array.Find(devices, d => d.MAC == mac);

            if (device == null && ApplyDesignatedId)
            {
                if (int.TryParse(mac.Substring(3), out int desiredId))
                {
                    device = Array.Find(devices, d => d.DesignatedId == desiredId);
                }
            }

            return device;
        }

        #endregion Networks

        #region Store

        public bool TryBuySoftware(Software software, out string message)
        {
            message = string.Empty;
            if (MoneyAmmount < software.Price)
            {
                message = "Not enough money!";
                return false;
            }

            Store.SoftwareBought(software);
            if(!Computer.StoreSoftware(software))
            {
                message = $"Not enough space to store {software.Name}";
                return false;
            }

            foreach (var item in software.Provides)
            {
                if (!AvailableSoftware.Contains(item.CommandName))
                {
                    AvailableSoftware.Add(item.CommandName);
                }

                if (item.Provide != CommandOptions.Invalid && item.Provide != CommandOptions.None)
                {
                    AvailableSoftwareOptions.Add(item.Provide);
                }
            }

            UpdateAmmount(-software.Price);
            return true;
        }

        public bool TryBuyComponent(StoreComponent component, out string message)
        {
            if (MoneyAmmount < component.Price)
            {
                message = "Not enough money";
                return false;
            }

            Store.ComponentBought(component);
            return Computer.UpdateComponent(component.SoldComponent, out message);
        }

        #endregion Store

        private void SendMessage(string message, MessageType type)
        {
            MessageSender?.Invoke(message, type);
        }

        public void ClearConsole()
        {
            ClearHandler?.Invoke();
        }

        public int GetComputerSpeed()
        {
            return Computer.GetExecutionSpeed();
        }

        public long GetNetworkSpeed()
        {
            return Computer.GetNetworkSpeed();
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
