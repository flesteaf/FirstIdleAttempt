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
        private readonly Dictionary<CommandOptions, Action<SceneManager>> showTypes;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.networks,
                            CommandOptions.ips };
        }

        public ShowCommand()
        {
            showTypes = new Dictionary<CommandOptions, Action<SceneManager>>
            {
                { CommandOptions.networks,  ShowNetworks },
                { CommandOptions.ips, ShowDevices }
            };
        }

        public override void Execute(SceneManager game, CommandLine command)
        {
            IConsoleText console = game.Console;
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

        private void ShowDevices(SceneManager game)
        {
            IEnumerable<Device> devices = game.GetAllHackedDevices();

            foreach (var item in devices)
            {
                game.Console.AddMessage(item.ToString(), MessageType.Info);
            }
        }

        private void ShowNetworks(SceneManager game)
        {
            IEnumerable<HackableNetwork> networks = game.GetAllFoundNetworks();

            foreach (var item in networks)
            {
                game.Console.AddMessage(item.ToString(), MessageType.Info);
            }
        }

        #endregion ShowCommands
    }
}