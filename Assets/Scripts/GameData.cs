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

        public List<CommandNames> AvailableSoftware { get; }
        public List<CommandOptions> AvailableSoftwareOptions { get; }
        public float CurrentProduction { get; private set; } = 0;
        public float MoneyAmmount { get; private set; } = 0;
        public List<HackableNetwork> FoundNetworks { get; }
        public Computer Computer { get; }
        public IGameStore Store { get; }

        public event SendMessageEventHandler MessageSender;

        public GameData(IGameStore store)
        {
            Store = store;
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
            networkFactory = new NetworkFactory();
            random = new Random();
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
                HackableNetwork item = networkFactory.GetRandomNetwork(NetworkType.Medium);
                AddNetwork(item);
                SendMessage(item.ToString(), MessageType.Info);
            }
        }

        public IEnumerable<Device> GetAllHackedDevices()
        {
            return FoundNetworks.SelectMany(n => n.Devices).Where(d => d.IsInfected);
        }

        public Device GetDeviceByIp(string ip)
        {
            Device[] devices = FoundNetworks.SelectMany(n => n.Devices).ToArray();
            return Array.Find(devices, d => d.IP == ip);
        }

        public HackableNetwork GetNetworkBySSID(string ssid)
        {
            HackableNetwork network = FoundNetworks.Find(n => n.SSID.Equals(ssid, StringComparison.OrdinalIgnoreCase));
            if (network != null && network.Protection == ProtectionType.None && !network.WasHacked)
            {
                network.HackNetwork(ProtectionType.None);
            }

            return network;
        }

        public Device GetDeviceByMac(string mac)
        {
            Device[] devices = FoundNetworks.SelectMany(n => n.Devices).ToArray();
            return Array.Find(devices, d => d.MAC == mac);
        }

        #endregion Networks

        #region Store

        public bool TryBuySoftware(Software software)
        {
            if (MoneyAmmount < software.Price)
            {
                return false;
            }

            Store.SoftwareBought(software);

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

        public bool TryBuyComponent(StoreComponent component)
        {
            if (MoneyAmmount < component.Price)
            {
                return false;
            }

            Store.ComponentBought(component);
            return Computer.UpdateComponent(component.SoldComponent);
        }

        #endregion Store

        private void SendMessage(string message, MessageType type)
        {
            MessageSender?.Invoke(message, type);
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
