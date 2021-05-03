using Assets.Scripts.Commands;
using Assets.Scripts.Computers;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public interface IGameLogic
    {
        PlayerData PlayerData { get; }

        bool ApplyDesignatedId { get; }
        bool CommandUnderExecution { get; set; }

        event HackerDelegates.SendMessageEventHandler MessageSender;
        event HackerDelegates.ClearConsoleEventHandler ClearHandler;

        void AddNetwork(HackableNetwork network);
        void AddProduction();
        IEnumerable<Device> GetAllHackedDevices();
        Device GetDeviceByIp(string ip);
        Device GetDeviceByMac(string mac);
        HackableNetwork GetNetworkBySSID(string ssid);
        void RefreshNetworks();
        bool TryBuyComponent(StoreComponent component, out string message);
        bool TryBuySoftware(Software software, out string message);
        void UpdateAmmount(float value);
        void UpdateCurrentProduction(float addValue);
        void ClearConsole();
        int GetComputerSpeed();
        long GetNetworkSpeed();
        void SavePlayerToCurrentSlot();
        void ClearSaveSlot();
        void AddMultiProduction(float totalSeconds);
    }
}