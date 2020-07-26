﻿using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class FirewallCommand : Command
    {
        public override CommandNames Name => CommandNames.firewall;
        private readonly Dictionary<CommandOptions, Action<SceneManager, string>> firewallOptions;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.enable,
                            CommandOptions.disable };
        }

        public FirewallCommand()
        {
            firewallOptions = new Dictionary<CommandOptions, Action<SceneManager, string>>
            {
                { CommandOptions.enable, EnableFirewall },
                { CommandOptions.disable, DisableFirewall }
            };
        }

        public override void Execute(SceneManager game, CommandLine command)
        {
            IConsoleText console = game.Console;

            if (!command.HasArgumentAndOption())
            {
                console.AddMessage("The firewall command receives 2 parameters: action (enable or disable) and ip or mac of the device", MessageType.Warning);
                return;
            }

            if (!firewallOptions.ContainsKey(command.Option))
            {
                console.AddMessage($"Wrong option selected. Option {command.Option} is unrecognized", MessageType.Error);
                return;
            }

            firewallOptions[command.Option](game, command.Argument);
        }

        private void DisableFirewall(SceneManager manager, string identifier)
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

        private void EnableFirewall(SceneManager manager, string identifier)
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
        
        private Device GetDevice(SceneManager manager, string identifier)
        {
            Device device = manager.GetDeviceByIp(identifier);
            if (device == null)
            {
                device = manager.GetDeviceByMac(identifier);
                if (device == null)
                {
                    manager.Console.AddMessage($"Device {identifier} not found", MessageType.Error);
                }
            }

            return device;
        }
    }
}