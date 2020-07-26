using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    public class ScanCommand : Command
    {
        public override CommandNames Name => CommandNames.scan;
        private readonly Dictionary<CommandOptions, Action<SceneManager, string>> scanTypes;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.network,
                            CommandOptions.ip,
                            CommandOptions.mac };
        }

        public ScanCommand()
        {
            scanTypes = new Dictionary<CommandOptions, Action<SceneManager, string>>
            {
                { CommandOptions.network, ScanNetwork },
                { CommandOptions.ip, ScanIp },
                { CommandOptions.mac, ScanMac }
            };
        }

        public override void Execute(SceneManager game, CommandLine command)
        {
            if (!command.HasArgument())
            {
                ScanForNetworks(game);
                return;
            }

            if (!command.HasArgumentAndOption())
            {
                game.Console.AddMessage("Invalid command call, the scan command takes 2 parameters at most. Check help command for scan", MessageType.Error);
                return;
            }

            if (!scanTypes.ContainsKey(command.Option))
            {
                game.Console.AddMessage("Invalid parameter as input for scan command, accepted are 'network', 'ip' and 'mac'", MessageType.Error);
                return;
            }

            scanTypes[command.Option](game, command.Argument);
        }

        #region Scan commands

        private void ScanForNetworks(SceneManager game)
        {
            game.RefreshNetworks();
        }

        private void ScanIp(SceneManager game, string ip)
        {
            Device device = game.GetDeviceByIp(ip);

            if (device == null)
            {
                game.Console.AddMessage($"The provided ip {ip} was not found, please provide a different [IP]", MessageType.Warning);
                return;
            }

            ProvideDeviceDetails(game, device);
        }

        private void ScanMac(SceneManager game, string mac)
        {
            Device device = game.GetDeviceByMac(mac);

            if (device == null)
            {
                game.Console.AddMessage($"The provided mac {mac} was not found, please provide a different [MAC]", MessageType.Warning);
                return;
            }

            ProvideDeviceDetails(game, device);
        }

        private void ProvideDeviceDetails(SceneManager game, Device device)
        {
            string firewallStatus = device.FirewallIsActive ? "enabled" : "disabled";
            game.Console.AddMessage($"Device has firewall {device.HasFirewall}", MessageType.Info);
            game.Console.AddMessage($"Firewall status is {firewallStatus}", MessageType.Info);
        }

        private void ScanNetwork(SceneManager game, string ssid)
        {
            HackableNetwork network = game.GetNetworkBySSID(ssid);

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
                game.Console.AddMessage($"- {item} | {item.Type}", MessageType.Info);
            }
        }

        #endregion Scan commands
    }
}