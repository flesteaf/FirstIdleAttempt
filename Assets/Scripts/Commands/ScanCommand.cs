﻿using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    public class ScanCommand : Command
    {
        public override CommandNames Name => CommandNames.scan;
        private readonly Dictionary<CommandOptions, Action<IGameData, string>> scanTypes;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.network,
                            CommandOptions.ip,
                            CommandOptions.mac };
        }

        public ScanCommand()
        {
            scanTypes = new Dictionary<CommandOptions, Action<IGameData, string>>
            {
                { CommandOptions.network, ScanNetwork },
                { CommandOptions.ip, ScanIp },
                { CommandOptions.mac, ScanMac }
            };
        }

        public override void Execute(IGameData game, CommandLine command)
        {
            if (!command.HasArgument())
            {
                game.RefreshNetworks();
                return;
            }

            if (!command.HasArgumentAndOption())
            {
                SendMessage("Invalid command call, the scan command takes 2 parameters at most. Check help command for scan", MessageType.Error);
                return;
            }

            if (!scanTypes.ContainsKey(command.Option))
            {
                SendMessage("Invalid parameter as input for scan command, accepted are 'network', 'ip' and 'mac'", MessageType.Error);
                return;
            }

            scanTypes[command.Option](game, command.Argument);
        }

        #region Scan commands

        private void ScanIp(IGameData game, string ip)
        {
            Device device = game.GetDeviceByIp(ip);

            if (device == null)
            {
                SendMessage($"The provided ip {ip} was not found, please provide a different [IP]", MessageType.Warning);
                return;
            }

            ProvideDeviceDetails(device);
        }

        private void ScanMac(IGameData game, string mac)
        {
            Device device = game.GetDeviceByMac(mac);

            if (device == null)
            {
                SendMessage($"The provided mac {mac} was not found, please provide a different [MAC]", MessageType.Warning);
                return;
            }

            ProvideDeviceDetails(device);
        }

        private void ProvideDeviceDetails(Device device)
        {
            string firewallStatus = device.FirewallIsActive ? "enabled" : "disabled";
            string hasFirewall = device.HasFirewall ? $"has firewall with status {firewallStatus}" : "doesn't have a firewall";
            SendMessage($"Device {hasFirewall}", MessageType.Info);
        }

        private void ScanNetwork(IGameData game, string ssid)
        {
            HackableNetwork network = game.GetNetworkBySSID(ssid);

            if (network == null)
            {
                SendMessage($"The provided SSID {ssid} was not found, please provide a different [SSID]", MessageType.Warning);
                return;
            }

            if (network.Protection != ProtectionType.None)
            {
                SendMessage($"The network {ssid} is protected. Crack the protection then try again.", MessageType.Warning);
                return;
            }

            SendMessage($"Network {network} has the following devices:", MessageType.Info);
            foreach (Device item in network.Devices)
            {
                SendMessage($"-> {item.ToString(game.ApplyDesignatedId)} | {item.Type}", MessageType.Info);
            }
        }

        #endregion Scan commands
    }
}