using Assets.Scripts.Computers.ComponentTypes;
using Assets.Scripts.Networks.Devices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class FirewallCommand : CommandWithDelay
    {
        private readonly Dictionary<CommandOptions, Func<IGameData, string, IEnumerator>> firewallOptions;
        private long delayExecutionTime;
        public override CommandNames Name => CommandNames.firewall;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.enable,
                            CommandOptions.disable };
        }

        protected override int BaseExecutionTime => 2000;

        public FirewallCommand()
        {
            firewallOptions = new Dictionary<CommandOptions, Func<IGameData, string, IEnumerator>>
            {
                { CommandOptions.enable, EnableFirewall },
                { CommandOptions.disable, DisableFirewall }
            };
        }

        public override IEnumerator Execute(IGameData game, CommandLine command, long delayTime)
        {
            delayExecutionTime = delayTime;
            if (!command.HasArgumentAndOption())
            {
                SendMessage("The firewall command receives 2 parameters: action (enable or disable) and ip or mac of the device", MessageType.Warning);
                yield break;
            }

            if (!firewallOptions.ContainsKey(command.Option))
            {
                SendMessage($"Wrong option selected. Option {command.Option} is unrecognized", MessageType.Error);
                yield break;
            }

            yield return firewallOptions[command.Option](game, command.Argument);
        }

        private IEnumerator DisableFirewall(IGameData manager, string identifier)
        {
            Device device = GetDevice(manager, identifier);
            if (device == null) {
                yield break;
            }

            if (device.FirewallIsActive)
            {
                yield return ExecuteDelay(delayExecutionTime, device.DeactivateFirewall);
            }
            yield break;
        }

        private IEnumerator EnableFirewall(IGameData manager, string identifier)
        {
            Device device = GetDevice(manager, identifier);
            if (device == null)
            {
                yield break;
            }

            if (device.HasFirewall && !device.FirewallIsActive)
            {
                yield return ExecuteDelay(delayExecutionTime, device.DeactivateFirewall);
            }
            yield break;
        }
        
        private Device GetDevice(IGameData manager, string identifier)
        {
            Device device = manager.GetDeviceByIp(identifier);
            if (device == null)
            {
                device = manager.GetDeviceByMac(identifier);
                if (device == null)
                {
                    SendMessage($"Device {identifier} not found", MessageType.Error);
                }
            }

            return device;
        }

        protected override long GetCommandDelay(int computerSpeed, long networkSpeed)
        {
            return BaseExecutionTime / computerSpeed + (long)Sizes.KB / networkSpeed;
        }
    }
}