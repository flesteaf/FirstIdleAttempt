using Assets.Scripts.Commands;
using Assets.Scripts.Computers;
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

public class SceneManager : MonoBehaviour, ISceneManager
{
    private readonly List<CommandNames> AvailableSoftware;
    private readonly List<CommandOptions> AvailableSoftwareOptions;
    private readonly List<HackableNetwork> foundNetworks;
    private readonly NetworkFactory networkFactory;
    private readonly Random random;
    private readonly float oneSecond = 1;
    private readonly float moneyGenerationExponent = 2.912f;
    private Store Store;
    private float currentProduction;
    private ConsoleText consoleText;
    private MissionText missionsText;
    private MoneyText moneyText;
    internal float MoneyAmmount = 0;

    public Computer Computer { get; }
    public IConsoleText Console { get => consoleText; }
    public IMissionText Mission { get => missionsText; }
    public IMoneyText Money { get => moneyText; }

    public SceneManager()
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

        foundNetworks = new List<HackableNetwork>();
        networkFactory = new NetworkFactory();
        random = new Random();
        currentProduction = 0;
    }

    private void Start()
    {
        moneyText.UpdateText(MoneyAmmount);
    }

    private void Awake()
    {
        //This can be done only in the main scene);
        consoleText = FindObjectOfType<ConsoleText>();
        missionsText = FindObjectOfType<MissionText>();
        moneyText = FindObjectOfType<MoneyText>();

        TextAsset dataAsset = (TextAsset)Resources.Load("dataStore");
        Store = JsonConvert.DeserializeObject<Store>(dataAsset.text);
        Time.fixedDeltaTime = oneSecond;
    }

    public void ExecuteCommand(CommandLine command)
    {
        consoleText.AddMessage(command.ToString(), MessageType.Info);

        Command action = CommandFactory.GetCommand(command);
        if (!AvailableSoftware.Contains(action.Name))
        {
            consoleText.AddMessage($"Command {action.Name} needs to be bought", MessageType.Error);
            return;
        }

        CommandOptions option = action.GetOptionFromCommand(command);
        if (option == CommandOptions.Invalid || !AvailableSoftwareOptions.Contains(option))
        {
            consoleText.AddMessage($"Option {option} of command {action.Name} needs to be bought", MessageType.Error);
            return;
        }
        
        action.Execute(this, command);
    }

    private void FixedUpdate()
    {
        MoneyAmmount += currentProduction;
        moneyText.UpdateText(MoneyAmmount);
    }

    #region Networks

    public void RefreshNetworks()
    {
        foundNetworks.RemoveAll(n => !n.WasHacked);

        int noOfNetworksToDiscover = random.Next(1, 5);
        for (int i = 0; i < noOfNetworksToDiscover; i++)
        {
            HackableNetwork item = networkFactory.GetRandomNetwork(NetworkType.Medium);
            item.NetworkHacked += NetworkHacked;
            foundNetworks.Add(item);
            consoleText.AddMessage(item.ToString(), MessageType.Info);
        }
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
            currentProduction += infectedDevice.EnergyLevel * moneyGenerationExponent;
    }

    public IEnumerable<HackableNetwork> GetAllFoundNetworks()
    {
        return foundNetworks;
    }

    public IEnumerable<Device> GetAllHackedDevices()
    {
        return foundNetworks.SelectMany(n => n.Devices).Where(d => d.IsInfected);
    }

    public Device GetDeviceByIp(string ip)
    {
        Device[] devices = foundNetworks.SelectMany(n => n.Devices).ToArray();
        return Array.Find(devices, d => d.IP == ip);
    }

    public HackableNetwork GetNetworkBySSID(string ssid)
    {
        HackableNetwork network = foundNetworks.Find(n => n.SSID.Equals(ssid, StringComparison.OrdinalIgnoreCase));
        if (network != null && network.Protection == ProtectionType.None)
        {
            NetworkHacked(network);
        }

        return network;
    }

    public Device GetDeviceByMac(string mac)
    {
        Device[] devices = foundNetworks.SelectMany(n => n.Devices).ToArray();
        return Array.Find(devices, d => d.MAC == mac);
    }

    #endregion Networks

    #region Store

    public IEnumerable<Software> GetAllSoftwares()
    {
        return Store.Softwares;
    }

    public IEnumerable<RamStore> GetStoreRams()
    {
        return Store.RAMs;
    }

    public IEnumerable<GpuStore> GetStoreGpus()
    {
        return Store.GPUs;
    }

    public IEnumerable<HardStore> GetStoreHards()
    {
        return Store.Hards;
    }

    public IEnumerable<MotherboardStore> GetStoreMotherboards()
    {
        return Store.Motherboards;
    }

    public IEnumerable<SourceStore> GetStoreSources()
    {
        return Store.Sources;
    }

    public IEnumerable<NetworkBoardStore> GetStoreNetworkBoards()
    {
        return Store.Networks;
    }

    public IEnumerable<CpuStore> GetStoreCpus()
    {
        return Store.CPUs;
    }

    public StoreComponent GetStoreComponent(string componentName)
    {
        return Store.GetComponent(componentName);
    }

    public Software GetStoreSoftware(string softwareName)
    {
        return Store.GetSoftware(softwareName);
    }

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

        MoneyAmmount -= software.Price;
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

    ~SceneManager()
    {
        foundNetworks.RemoveAll(hn => !hn.WasHacked);
        foundNetworks.SelectMany(n => n.Devices).ToList().ForEach(d => { if (d.IsInfected) d.DeviceInfected -= DeviceInfected; });
        foundNetworks.ForEach(hn => hn.NetworkHacked -= NetworkHacked);
        foundNetworks.RemoveAll(hn => true);
    }
}