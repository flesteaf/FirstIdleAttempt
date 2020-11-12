using Assets.Scripts.Computers.ComponentTypes;
using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    public class ScanCommand : CommandWithDelay
    {
        private readonly Dictionary<CommandOptions, Func<IGameData, string, IEnumerator>> scanTypes;
        private readonly float networkCommunication = 0.1f*(long)Sizes.KB;
        private int delayExecutionTime;

        public override CommandNames Name => CommandNames.scan;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.network,
                            CommandOptions.ip,
                            CommandOptions.mac };
        }

        protected override int BaseExecutionTime => 2000;

        public ScanCommand()
        {
            scanTypes = new Dictionary<CommandOptions, Func<IGameData, string, IEnumerator>>
            {
                { CommandOptions.network, ScanNetwork },
                { CommandOptions.ip, ScanIp },
                { CommandOptions.mac, ScanMac }
            };
        }

        public override IEnumerator Execute(IGameData game, CommandLine command, int delayTime)
        {
            delayExecutionTime = delayTime;
            if (!command.HasArgument())
            {
                yield return ExecuteDelay(delayTime, game.RefreshNetworks);
                yield break;
            }

            if (!command.HasArgumentAndOption())
            {
                SendMessage("Invalid command call, the scan command takes 2 parameters at most. Check help command for scan", MessageType.Error);
                yield break;
            }

            if (!scanTypes.ContainsKey(command.Option))
            {
                SendMessage("Invalid parameter as input for scan command, accepted are 'network', 'ip' and 'mac'", MessageType.Error);
                yield break;
            }

            yield return scanTypes[command.Option](game, command.Argument);
        }

        #region Scan commands

        private IEnumerator ScanIp(IGameData game, string ip)
        {
            Device device = game.GetDeviceByIp(ip);

            if (device == null)
            {
                SendMessage($"The provided ip {ip} was not found, please provide a different [IP]", MessageType.Warning);
                yield break;
            }

            yield return ExecuteDelay(delayExecutionTime + 1000, ProvideDeviceDetails, device);
        }

        private IEnumerator ScanMac(IGameData game, string mac)
        {
            Device device = game.GetDeviceByMac(mac);

            if (device == null)
            {
                SendMessage($"The provided mac {mac} was not found, please provide a different [MAC]", MessageType.Warning);
                yield break;
            }

            yield return ExecuteDelay(delayExecutionTime + 1000, ProvideDeviceDetails, device);
        }

        private void ProvideDeviceDetails(Device device)
        {
            string firewallStatus = device.FirewallIsActive ? "enabled" : "disabled";
            string hasFirewall = device.HasFirewall ? $"has firewall with status {firewallStatus}" : "doesn't have a firewall";
            SendMessage($"Device {hasFirewall}", MessageType.Info);
        }

        private IEnumerator ScanNetwork(IGameData game, string ssid)
        {
            HackableNetwork network = game.GetNetworkBySSID(ssid);

            if (network == null)
            {
                SendMessage($"The provided SSID {ssid} was not found, please provide a different [SSID]", MessageType.Warning);
                yield break;
            }

            if (network.Protection != ProtectionType.None)
            {
                SendMessage($"The network {ssid} is protected. Crack the protection then try again.", MessageType.Warning);
                yield break;
            }

            yield return ExecuteDelay(delayExecutionTime + 3000, ListDevices, game, network);
        }

        private void ListDevices(IGameData game, HackableNetwork network)
        {
            SendMessage($"Network {network} has the following devices:", MessageType.Info);
            foreach (Device item in network.Devices)
            {
                SendMessage($"-> {item.ToString(game.ApplyDesignatedId)} | {item.Type}", MessageType.Info);
            }
        }

        #endregion Scan commands

        protected override int GetCommandDelay(int computerSpeed, long networkSpeed)
        {
            return BaseExecutionTime / computerSpeed + (int)(networkCommunication / networkSpeed);
        }
    }
}