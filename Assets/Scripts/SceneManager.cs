using Assets.Scripts;
using Assets.Scripts.Commands;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class SceneManager : MonoBehaviour
{
    private readonly NetworkFactory networkFactory;
    private readonly Random random;
    private readonly float oneSecond = 1;

    public ConsoleText Console { get; private set; }
    public MissionText Mission { get; private set; }
    public MoneyText Money { get; private set; }
    public Store Store { get; private set; }
    public GameData Data { get; private set; }

    public SceneManager()
    {
        networkFactory = new NetworkFactory();
        random = new Random();
        Data = new GameData();
    }

    private void Start()
    {
        Money.UpdateText(Data.MoneyAmmount);
    }

    private void Awake()
    {
        //This can be done only in the main scene);
        Console = FindObjectOfType<ConsoleText>();
        Mission = FindObjectOfType<MissionText>();
        Money = FindObjectOfType<MoneyText>();

        TextAsset dataAsset = (TextAsset)Resources.Load("dataStore");
        Store = JsonConvert.DeserializeObject<Store>(dataAsset.text);
        Time.fixedDeltaTime = oneSecond;
    }

    public void ExecuteCommand(CommandLine command)
    {
        Console.AddMessage(command.ToString(), MessageType.Info);

        Command action = CommandFactory.GetCommand(command);
        if (!Data.AvailableSoftware.Contains(action.Name))
        {
            Console.AddMessage($"Command {action.Name} needs to be bought", MessageType.Error);
            return;
        }

        CommandOptions option = action.GetOptionFromCommand(command);
        if (option == CommandOptions.Invalid || !Data.AvailableSoftwareOptions.Contains(option))
        {
            Console.AddMessage($"Option {option} of command {action.Name} needs to be bought", MessageType.Error);
            return;
        }

        action.MessageNotification += SendToConsole;
        action.Execute(Data, command);
        action.MessageNotification -= SendToConsole;
    }

    private void SendToConsole(string message, MessageType type)
    {
        Console.AddMessage(message, type);
    }

    private void FixedUpdate()
    {
        Data.AddProduction();
        Money.UpdateText(Data.MoneyAmmount);
    }

    #region Networks

    public void RefreshNetworks()
    {
        Data.FoundNetworks.RemoveAll(n => !n.WasHacked);

        int noOfNetworksToDiscover = random.Next(1, 5);
        for (int i = 0; i < noOfNetworksToDiscover; i++)
        {
            HackableNetwork item = networkFactory.GetRandomNetwork(NetworkType.Medium);
            Data.AddNetwork(item);
            Console.AddMessage(item.ToString(), MessageType.Info);
        }
    }

    public IEnumerable<Device> GetAllHackedDevices()
    {
        return Data.FoundNetworks.SelectMany(n => n.Devices).Where(d => d.IsInfected);
    }

    public Device GetDeviceByIp(string ip)
    {
        Device[] devices = Data.FoundNetworks.SelectMany(n => n.Devices).ToArray();
        return Array.Find(devices, d => d.IP == ip);
    }

    public HackableNetwork GetNetworkBySSID(string ssid)
    {
        HackableNetwork network = Data.FoundNetworks.Find(n => n.SSID.Equals(ssid, StringComparison.OrdinalIgnoreCase));
        if (network != null && network.Protection == ProtectionType.None && !network.WasHacked)
        {
            network.HackNetwork(ProtectionType.None);
        }

        return network;
    }

    public Device GetDeviceByMac(string mac)
    {
        Device[] devices = Data.FoundNetworks.SelectMany(n => n.Devices).ToArray();
        return Array.Find(devices, d => d.MAC == mac);
    }

    #endregion Networks

    #region Store

    public bool TryBuySoftware(Software software)
    {
        if (Data.MoneyAmmount < software.Price)
        {
            return false;
        }

        Store.SoftwareBought(software);

        foreach (var item in software.Provides)
        {
            if (!Data.AvailableSoftware.Contains(item.CommandName))
            {
                Data.AvailableSoftware.Add(item.CommandName);
            }

            if (item.Provide != CommandOptions.Invalid && item.Provide != CommandOptions.None)
            {
                Data.AvailableSoftwareOptions.Add(item.Provide); 
            }
        }

        Data.UpdateAmmount(-software.Price);
        return true;
    }

    public bool TryBuyComponent(StoreComponent component)
    {
        if (Data.MoneyAmmount < component.Price)
        {
            return false;
        }

        Store.ComponentBought(component);
        return Data.Computer.UpdateComponent(component.SoldComponent);
    }

    #endregion Store
}