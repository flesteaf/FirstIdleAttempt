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
    public class GameLogic : IGameLogic
    {
        private readonly float moneyGenerationExponent = 2.912f;
        private readonly NetworkFactory networkFactory;
        private static bool commandUnderExecution;

        public PlayerData PlayerData { get; }

        public bool ApplyDesignatedId => PlayerData.AvailableSoftware.Contains(CommandNames.extract);

        public bool CommandUnderExecution { get => commandUnderExecution; set => commandUnderExecution = value; }

        public event SendMessageEventHandler MessageSender;
        public event ClearConsoleEventHandler ClearHandler;

        public GameLogic(PlayerData playerData)
        {
            PlayerData = playerData;
            
            if (PlayerData.NewPlayer)
            {
                IntializeNewPlayer();
            }

            networkFactory = new NetworkFactory();
            commandUnderExecution = false;
        }

        private void IntializeNewPlayer()
        {
            PlayerData.Computer = new InitialComputer();
            PlayerData.AvailableSoftwareOptions = new List<CommandOptions> {
                    CommandOptions.None,
                    CommandOptions.Invalid,
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
                    CommandOptions.software,
                    CommandOptions.clear};

            PlayerData.AvailableSoftware = new List<CommandNames> {
                    CommandNames.help,
                    CommandNames.status,
                    CommandNames.store,
                    CommandNames.buy,
                    CommandNames.scan,
                    CommandNames.inject,
                    CommandNames.show,
                    CommandNames.clear,
                    CommandNames.save,
                    CommandNames.invalid};

            PlayerData.FoundNetworks = new List<HackableNetwork>();
            PlayerData.NewPlayer = false;
        }

        public void UpdateCurrentProduction(float addValue)
        {
            PlayerData.CurrentProduction += addValue * moneyGenerationExponent;
        }

        public void UpdateAmmount(float value)
        {
            PlayerData.MoneyAmmount += value;
        }

        public void AddProduction()
        {
            PlayerData.MoneyAmmount += PlayerData.CurrentProduction;
        }

        public void AddMultiProduction(float times)
        {
            PlayerData.MoneyAmmount += (PlayerData.CurrentProduction * times);
        }

        #region Networks

        public void AddNetwork(HackableNetwork network)
        {
            network.NetworkHacked += NetworkHacked;
            PlayerData.FoundNetworks.Add(network);
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
            PlayerData.FoundNetworks.RemoveAll(n => !n.WasHacked);

            int noOfNetworksToDiscover = new Random().Next(1, 5);
            for (int i = 0; i < noOfNetworksToDiscover; i++)
            {
                HackableNetwork item = networkFactory.GetRandomNetwork(ApplyDesignatedId);

                AddNetwork(item);
                SendMessage(item.ToString(ApplyDesignatedId), MessageType.Info);
            }
        }

        public IEnumerable<Device> GetAllHackedDevices()
        {
            return PlayerData.FoundNetworks.SelectMany(n => n.Devices).Where(d => d.IsInfected);
        }

        public Device GetDeviceByIp(string ip)
        {
            Device[] devices = PlayerData.FoundNetworks.SelectMany(n => n.Devices).ToArray();
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
            HackableNetwork network = PlayerData.FoundNetworks.Find(n => n.SSID.Equals(ssid, StringComparison.OrdinalIgnoreCase));
            if (network == null && ApplyDesignatedId)
            {
                if (int.TryParse(ssid.Substring(3), out int desiredId))
                {
                    network = PlayerData.FoundNetworks.Find(n => n.DesignatedId == desiredId);
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
            Device[] devices = PlayerData.FoundNetworks.SelectMany(n => n.Devices).ToArray();
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
            if (PlayerData.MoneyAmmount < software.Price)
            {
                message = "Not enough money!";
                return false;
            }

            if (!PlayerData.Computer.StoreSoftware(software))
            {
                message = $"Not enough space to store {software.Name}";
                return false;
            }

            foreach (var item in software.Provides)
            {
                if (!PlayerData.AvailableSoftware.Contains(item.CommandName))
                {
                    PlayerData.AvailableSoftware.Add(item.CommandName);
                }

                if (item.Provide != CommandOptions.Invalid && item.Provide != CommandOptions.None)
                {
                    PlayerData.AvailableSoftwareOptions.Add(item.Provide);
                }
            }

            UpdateAmmount(-software.Price);
            return true;
        }

        public bool TryBuyComponent(StoreComponent component, out string message)
        {
            if (PlayerData.MoneyAmmount < component.Price)
            {
                message = "Not enough money";
                return false;
            }

            if (PlayerData.Computer.UpdateComponent(component.SoldComponent, out message))
            {
                UpdateAmmount(-component.Price);
                return true;
            }

            return false;
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
            return PlayerData.Computer.GetExecutionSpeed();
        }

        public long GetNetworkSpeed()
        {
            return PlayerData.Computer.GetNetworkSpeed();
        }

        public void SavePlayerToCurrentSlot()
        {
            SaveManager.SavePlayer(PlayerData);
        }

        public void ClearSaveSlot()
        {
            SaveManager.ClearSlot();
        }

        ~GameLogic()
        {
            PlayerData.FoundNetworks.RemoveAll(hn => !hn.WasHacked);
            PlayerData.FoundNetworks.SelectMany(n => n.Devices).ToList().ForEach(d => { if (d.IsInfected) d.DeviceInfected -= DeviceInfected; });
            PlayerData.FoundNetworks.ForEach(hn => hn.NetworkHacked -= NetworkHacked);
            PlayerData.FoundNetworks.RemoveAll(hn => true);
        }
    }
}
