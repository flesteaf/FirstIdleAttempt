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

public class GameManager : MonoBehaviour
{
    public CommandLineField InputText;
    public ConsoleText Console;
    public MissionText Missions;
    public MoneyText Money;
    public readonly Computer Computer;
    public readonly List<CommandOptions> AvailableSoftwareOptions;
    public readonly List<CommandNames> AvailableSoftware;

    private float moneyAmmount = 0;
    private readonly List<HackableNetwork> foundNetworks;
    private readonly NetworkFactory networkFactory;
    private readonly Random random;
    private Store Store;
    private float currentProduction;
    private float oneSecond = 1;

    public GameManager()
    {
        Computer = new InitialComputer();
        AvailableSoftwareOptions = new List<CommandOptions> {
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

    private void Awake()
    {
        //This can be done only in the main scene
        InputText = FindObjectOfType<CommandLineField>();
        Console = FindObjectOfType<ConsoleText>();
        Missions = FindObjectOfType<MissionText>();
        Money = FindObjectOfType<MoneyText>();

        TextAsset dataAsset = (TextAsset)Resources.Load("dataStore");
        Store = JsonConvert.DeserializeObject<Store>(dataAsset.text);
        Time.fixedDeltaTime = oneSecond;
    }

    // Start is called before the first frame update
    private void Start()
    {
        Money.UpdateText(moneyAmmount);
    }

    public void ExecuteCommand(string command)
    {
        Console.AddMessage(command, MessageType.Info);
        Command action = CommandFactory.GetCommand(command);
        action.Execute(this, command);
    }

    private void FixedUpdate()
    {
        moneyAmmount += currentProduction;
        Money.UpdateText(moneyAmmount);
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
            Console.AddMessage(item.ToString(), MessageType.Info);
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
            currentProduction += infectedDevice.EnergyLevel * 0.12f;
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

    public HackableNetwork GetNetwork(string ssid)
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
        if (moneyAmmount < software.Price)
        {
            return false;
        }

        Store.SoftwareBought(software);

        if (!AvailableSoftware.Contains(software.CommandName))
        {
            AvailableSoftware.Add(software.CommandName);
        }

        AvailableSoftwareOptions.Add(software.Provides);
        return true;
    }

    public bool TryBuyComponent(StoreComponent component)
    {
        if (moneyAmmount < component.Price)
        {
            return false;
        }

        Store.ComponentBought(component);
        return Computer.UpdateComponent(component.SoldComponent);
    }

    #endregion Store

    ~GameManager()
    {
        foundNetworks.RemoveAll(hn => !hn.WasHacked);
        foundNetworks.SelectMany(n => n.Devices).ToList().ForEach(d => { if (d.IsInfected) d.DeviceInfected -= DeviceInfected; });
        foundNetworks.ForEach(hn => hn.NetworkHacked -= NetworkHacked);
        foundNetworks.RemoveAll(hn => true);
    }
}