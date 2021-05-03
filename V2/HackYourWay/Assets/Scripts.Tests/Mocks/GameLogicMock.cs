using Assets.Scripts;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using System.Collections.Generic;

public class GameLogicMock : IGameLogic
{
    public string RequestedMethod { get; set; }
    public List<object> InputParameters { get; set; }
    public object OutputParameter { get;set; }

    public PlayerData PlayerData => throw new System.NotImplementedException();

    public bool ApplyDesignatedId => throw new System.NotImplementedException();

    public bool CommandUnderExecution { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public event HackerDelegates.SendMessageEventHandler MessageSender;
    public event HackerDelegates.ClearConsoleEventHandler ClearHandler;

    public void AddMultiProduction(float totalSeconds)
    {
        throw new System.NotImplementedException();
    }

    public void AddNetwork(HackableNetwork network)
    {
        throw new System.NotImplementedException();
    }

    public void AddProduction()
    {
        throw new System.NotImplementedException();
    }

    public void ClearConsole()
    {
        throw new System.NotImplementedException();
    }

    public void ClearSaveSlot()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<Device> GetAllHackedDevices()
    {
        throw new System.NotImplementedException();
    }

    public int GetComputerSpeed()
    {
        throw new System.NotImplementedException();
    }

    public Device GetDeviceByIp(string ip)
    {
        throw new System.NotImplementedException();
    }

    public Device GetDeviceByMac(string mac)
    {
        throw new System.NotImplementedException();
    }

    public HackableNetwork GetNetworkBySSID(string ssid)
    {
        throw new System.NotImplementedException();
    }

    public long GetNetworkSpeed()
    {
        throw new System.NotImplementedException();
    }

    public void RefreshNetworks()
    {
        RequestedMethod = nameof(RefreshNetworks);
        InputParameters = null;
        OutputParameter = null;
    }

    public void SavePlayerToCurrentSlot()
    {
        throw new System.NotImplementedException();
    }

    public bool TryBuyComponent(StoreComponent component, out string message)
    {
        throw new System.NotImplementedException();
    }

    public bool TryBuySoftware(Software software, out string message)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateAmmount(float value)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateCurrentProduction(float addValue)
    {
        throw new System.NotImplementedException();
    }
}
