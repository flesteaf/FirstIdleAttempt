﻿using Assets.Scripts.Commands;
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
    private float moneyAmmount = 0;
    internal CommandLineField InputText;
    internal ConsoleText Console;
    internal MissionText Missions;
    internal MoneyText Money;
    internal readonly Computer Computer;
    internal readonly List<CommandOptions> AvailableSoftwareOptions;
    internal readonly List<CommandNames> AvailableSoftware;
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

    internal void ExecuteCommand(string command)
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

    internal void RefreshNetworks()
    {
        foundNetworks.RemoveAll(n => !n.WasHacked);

        int noOfNetworksToDiscover = random.Next(1, 5);
        for (int i = 0; i < noOfNetworksToDiscover; i++)
        {
            HackableNetwork item = networkFactory.GetRandomNetwork(NetworkType.Small);
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

    internal IEnumerable<HackableNetwork> GetAllFoundNetworks()
    {
        return foundNetworks;
    }

    internal IEnumerable<Device> GetAllHackedDevices()
    {
        return foundNetworks.SelectMany(n => n.Devices).Where(d => d.IsInfected);
    }

    internal Device GetDeviceByIp(string ip)
    {
        Device[] devices = foundNetworks.SelectMany(n => n.Devices).ToArray();
        return Array.Find(devices, d => d.IP == ip);
    }

    internal HackableNetwork GetNetwork(string ssid)
    {
        HackableNetwork network = foundNetworks.Find(n => n.SSID.Equals(ssid, StringComparison.OrdinalIgnoreCase));
        if (network != null && network.Protection == ProtectionType.None)
        {
            NetworkHacked(network);
        }

        return network;
    }

    internal Device GetDeviceByMac(string mac)
    {
        Device[] devices = foundNetworks.SelectMany(n => n.Devices).ToArray();
        return Array.Find(devices, d => d.MAC == mac);
    }

    #endregion Networks

    #region Store

    internal IEnumerable<Software> GetAllSoftwares()
    {
        return Store.Softwares;
    }

    internal IEnumerable<RamStore> GetStoreRams()
    {
        return Store.RAMs;
    }

    internal IEnumerable<GpuStore> GetStoreGpus()
    {
        return Store.GPUs;
    }

    internal IEnumerable<HardStore> GetStoreHards()
    {
        return Store.Hards;
    }

    internal IEnumerable<MotherboardStore> GetStoreMotherboards()
    {
        return Store.Motherboards;
    }

    internal IEnumerable<SourceStore> GetStoreSources()
    {
        return Store.Sources;
    }

    internal IEnumerable<NetworkBoardStore> GetStoreNetworkBoards()
    {
        return Store.Networks;
    }

    internal IEnumerable<CpuStore> GetStoreCpus()
    {
        return Store.CPUs;
    }

    internal StoreComponent GetStoreComponent(string componentName)
    {
        return Store.GetComponent(componentName);
    }

    internal Software GetStoreSoftware(string softwareName)
    {
        return Store.GetSoftware(softwareName);
    }

    internal bool TryBuySoftware(Software software)
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

    internal bool TryBuyComponent(StoreComponent component)
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