using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class ScanCommand : Command
    {
        public override string Name => "scan";
        private readonly Dictionary<string, Action<GameManager, string>> scanTypes;

        public ScanCommand()
        {
            scanTypes = new Dictionary<string, Action<GameManager, string>>
            {
                { CommandOptions.network.ToString(), ScanNetwork },
                { CommandOptions.ip.ToString(), ScanIp },
                { CommandOptions.mac.ToString(), ScanMac }
            };
        }

        public override void Execute(GameManager game, string command)
        {
            string[] components = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (components.Length == 1)
            {
                ScanForNetworks(game);
                return;
            }

            if (components.Length != 3)
            {
                game.Console.AddMessage("Invalid command call, the scan command takes 2 parameters at most. Check help command for scan", MessageType.Error);
                return;
            }

            if (!scanTypes.ContainsKey(components[1]))
            {
                game.Console.AddMessage("Invalid parameter as input for scan command, accepted are 'network', 'ip' and 'mac'", MessageType.Error);
                return;
            }

            scanTypes[components[1]](game, components[2]);
        }

        #region Scan commands

        private void ScanForNetworks(GameManager game)
        {
            game.RefreshNetworks();
        }

        private void ScanIp(GameManager game, string ip)
        {
            Device device = game.GetDeviceByIp(ip);

            if (device == null)
            {
                game.Console.AddMessage($"The provided ip {ip} was not found, please provide a different [IP]", MessageType.Warning);
                return;
            }

            ProvideDeviceDetails(game, device);
        }

        private void ScanMac(GameManager game, string mac)
        {
            Device device = game.GetDeviceByMac(mac);

            if (device == null)
            {
                game.Console.AddMessage($"The provided mac {mac} was not found, please provide a different [MAC]", MessageType.Warning);
                return;
            }

            ProvideDeviceDetails(game, device);
        }

        private void ProvideDeviceDetails(GameManager game, Device device)
        {
            game.Console.AddMessage($"Device has firewall {device.HasFirewall}", MessageType.Info);
            game.Console.AddMessage($"Firewall status is {device.FirewallIsActive}", MessageType.Info);
        }

        private void ScanNetwork(GameManager game, string ssid)
        {
            HackableNetwork network = game.GetNetwork(ssid);

            if (network == null)
            {
                game.Console.AddMessage($"The provided SSID {ssid} was not found, please provide a different [SSID]", MessageType.Warning);
                return;
            }

            if (network.Protection != ProtectionType.None)
            {
                game.Console.AddMessage($"The network {ssid} is protected. Crack the protection then try again.", MessageType.Warning);
                return;
            }

            game.Console.AddMessage($"Network {network} has the following devices:", MessageType.Info);
            foreach (Device item in network.Devices)
            {
                game.Console.AddMessage($"- {item}", MessageType.Info);
            }
        }

        #endregion Scan commands
    }
}