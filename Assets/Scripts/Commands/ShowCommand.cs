using Assets.Scripts.Networks;
using Assets.Scripts.Networks.Devices;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class ShowCommand : Command
    {
        public override string Name => CommandNames.show.ToString();
        private readonly Dictionary<string, Action<GameManager>> showTypes;

        public ShowCommand()
        {
            showTypes = new Dictionary<string, Action<GameManager>>
            {
                { CommandOptions.networks.ToString(),  ShowNetworks },
                { CommandOptions.ips.ToString(), ShowDevices }
            };
        }

        public override void Execute(GameManager game, string command)
        {
            ConsoleText console = game.Console;
            var commandComponents = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commandComponents.Length < 2)
            {
                console.AddMessage("Show command provides details about either 'networks' or 'ips'.", MessageType.Warning);
                console.AddMessage("Please provide the option to show, e.g. 'show networks'", MessageType.Warning);
                return;
            }

            if (commandComponents.Length > 2)
            {
                console.AddMessage("Show command receives only 1 option parameter from 'netwroks' or 'ips'", MessageType.Warning);
                return;
            }

            if (!showTypes.ContainsKey(commandComponents[1]))
            {
                console.AddMessage($"Wrong option selected. Option {commandComponents[1]} is unrecognized", MessageType.Error);
                return;
            }

            showTypes[commandComponents[1]](game);
        }

        #region ShowCommands

        private void ShowDevices(GameManager game)
        {
            IEnumerable<Device> devices = game.GetAllHackedDevices();

            foreach (var item in devices)
            {
                game.Console.AddMessage(item.ToString(), MessageType.Info);
            }
        }

        private void ShowNetworks(GameManager game)
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