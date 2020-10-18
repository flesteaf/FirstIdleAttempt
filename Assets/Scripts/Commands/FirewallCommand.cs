using Assets.Scripts.Computers.ComponentTypes;
using Assets.Scripts.Networks.Devices;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class FirewallCommand : CommandWithDelay
    {
        private readonly Dictionary<CommandOptions, Action<IGameData, string>> firewallOptions;
        private int delayExecutionTime;
        public override CommandNames Name => CommandNames.firewall;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.enable,
                            CommandOptions.disable };
        }

        protected override int BaseExecutionTime => 5000;

        public FirewallCommand()
        {
            firewallOptions = new Dictionary<CommandOptions, Action<IGameData, string>>
            {
                { CommandOptions.enable, EnableFirewall },
                { CommandOptions.disable, DisableFirewall }
            };
        }

        public override void Execute(IGameData game, CommandLine command, int delayTime)
        {
            delayExecutionTime = delayTime;
            if (!command.HasArgumentAndOption())
            {
                SendMessage("The firewall command receives 2 parameters: action (enable or disable) and ip or mac of the device", MessageType.Warning);
                return;
            }

            if (!firewallOptions.ContainsKey(command.Option))
            {
                SendMessage($"Wrong option selected. Option {command.Option} is unrecognized", MessageType.Error);
                return;
            }

            firewallOptions[command.Option](game, command.Argument);
        }

        private void DisableFirewall(IGameData manager, string identifier)
        {
            Device device = GetDevice(manager, identifier);
            if (device == null) { 
                return;
            }

            if (device.FirewallIsActive)
            {
                device.DeactivateFirewall();
            }
        }

        private void EnableFirewall(IGameData manager, string identifier)
        {
            Device device = GetDevice(manager, identifier);
            if (device == null)
            {
                return;
            }

            if (device.HasFirewall && !device.FirewallIsActive)
            {
                device.DeactivateFirewall();
            }
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

        protected override int GetCommandDelay(int computerSpeed, long networkSpeed)
        {
            return BaseExecutionTime / computerSpeed + (int)((long)Sizes.MB / networkSpeed);
        }
    }
}