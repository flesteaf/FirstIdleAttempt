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
        private readonly Dictionary<CommandOptions, Action<GameManager>> showTypes;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.networks,
                            CommandOptions.ips };
        }

        public ShowCommand()
        {
            showTypes = new Dictionary<CommandOptions, Action<GameManager>>
            {
                { CommandOptions.networks,  ShowNetworks },
                { CommandOptions.ips, ShowDevices }
            };
        }

        public override void Execute(GameManager game, CommandLine command)
        {
            ConsoleText console = game.SceneManager.Console;
            if (!command.HasArgumentAndNoOption())
            {
                console.AddMessage("Show command provides details about either 'networks' or 'ips'.", MessageType.Warning);
                console.AddMessage("Please provide the option to show, e.g. 'show networks'", MessageType.Warning);
                return;
            }

            if (command.HasArgumentAndOption())
            {
                console.AddMessage("Show command receives only 1 option parameter from 'networks' or 'ips'", MessageType.Warning);
                return;
            }

            if (!showTypes.ContainsKey(command.ArgumentAsOption()))
            {
                console.AddMessage($"Wrong option selected. Option {command.ArgumentAsOption()} is unrecognized", MessageType.Error);
                return;
            }

            showTypes[command.ArgumentAsOption()](game);
        }

        #region ShowCommands

        private void ShowDevices(GameManager game)
        {
            IEnumerable<Device> devices = game.GetAllHackedDevices();

            foreach (var item in devices)
            {
                game.SceneManager.Console.AddMessage(item.ToString(), MessageType.Info);
            }
        }

        private void ShowNetworks(GameManager game)
        {
            IEnumerable<HackableNetwork> networks = game.GetAllFoundNetworks();

            foreach (var item in networks)
            {
                game.SceneManager.Console.AddMessage(item.ToString(), MessageType.Info);
            }
        }

        #endregion ShowCommands
    }
}