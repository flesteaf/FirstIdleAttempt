using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class ShowCommand : Command
    {
        public override CommandNames Name => CommandNames.show;
        private readonly Dictionary<CommandOptions, Action<GameData>> showTypes;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.networks,
                            CommandOptions.ips };
        }

        public ShowCommand()
        {
            showTypes = new Dictionary<CommandOptions, Action<GameData>>
            {
                { CommandOptions.networks,  ShowNetworks },
                { CommandOptions.ips, ShowDevices }
            };
        }

        public override void Execute(GameData game, CommandLine command)
        {
            if (!command.HasArgumentAndNoOption())
            {
                SendMessage("Show command provides details about either 'networks' or 'ips'.", MessageType.Warning);
                SendMessage("Please provide the option to show, e.g. 'show networks'", MessageType.Warning);
                return;
            }

            if (command.HasArgumentAndOption())
            {
                SendMessage("Show command receives only 1 option parameter from 'networks' or 'ips'", MessageType.Warning);
                return;
            }

            if (!showTypes.ContainsKey(command.ArgumentAsOption()))
            {
                SendMessage($"Wrong option selected. Option {command.ArgumentAsOption()} is unrecognized", MessageType.Error);
                return;
            }

            showTypes[command.ArgumentAsOption()](game);
        }

        #region ShowCommands

        private void ShowDevices(GameData game)
        {
            IEnumerable<Device> devices = game.GetAllHackedDevices();

            foreach (var item in devices)
            {
                SendMessage(item.ToString(), MessageType.Info);
            }
        }

        private void ShowNetworks(GameData game)
        {
            IEnumerable<HackableNetwork> networks = game.GetAllFoundNetworks();

            foreach (var item in networks)
            {
                SendMessage(item.ToString(), MessageType.Info);
            }
        }

        #endregion ShowCommands
    }
}