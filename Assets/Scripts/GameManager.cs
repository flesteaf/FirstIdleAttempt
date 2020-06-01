using Assets.Scripts.Commands;
using Assets.Scripts.Computers;
using Assets.Scripts.Networks;
using Random = System.Random;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using Assets.Scripts.Computers.Networks;
using System.Linq;
using Assets.Scripts.Networks.Devices;

public class GameManager : MonoBehaviour
{
    private float moneyAmmount = 100;
    internal CommandLineField InputText;
    internal ConsoleText Console;
    internal MissionText Missions;
    internal MoneyText Money;
    internal Computer Computer;
    private List<HackableNetwork> foundNetworks;
    private NetworkFactory networkFactory;
    private Random random;

    // Start is called before the first frame update
    private void Awake()
    {
        InputText = FindObjectOfType<CommandLineField>();
        Console = FindObjectOfType<ConsoleText>();
        Missions = FindObjectOfType<MissionText>();
        Money = FindObjectOfType<MoneyText>();
        Computer = new InitialComputer();

        foundNetworks = new List<HackableNetwork>();
        networkFactory = new NetworkFactory();
        random = new System.Random();
    }

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

    // Update is called once per frame
    private void Update()
    {
        moneyAmmount += 0.001f;
        Money.UpdateText(moneyAmmount);
    }

    internal void RefreshNetworks()
    {
        foundNetworks.RemoveAll(n => !n.WasHacked);

        int noOfNetworksToDiscover = random.Next(1, 5);
        for (int i = 0; i < noOfNetworksToDiscover; i++)
        {
            HackableNetwork item = networkFactory.GetRandomNetwork(NetworkType.Small);
            foundNetworks.Add(item);
            Console.AddMessage(item.ToString(), MessageType.Info);
        }
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

    internal IEnumerable<HackableNetwork> GetAllFoundNetworks()
    {
        return foundNetworks;
    }

    internal HackableNetwork GetNetwork(string ssid)
    {
        return foundNetworks.Find(n => n.SSID.Equals(ssid, StringComparison.OrdinalIgnoreCase));
    }

    internal Device GetDeviceByMac(string mac)
    {
        Device[] devices = foundNetworks.SelectMany(n => n.Devices).ToArray();
        return Array.Find(devices, d => d.MAC == mac);
    }
}