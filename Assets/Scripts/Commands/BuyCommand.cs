using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    public class BuyCommand : Command
    {
        private Dictionary<string, Action<GameManager, string>> buyOptions;
        public override string Name => "buy";

        public BuyCommand()
        {
            buyOptions = new Dictionary<string, Action<GameManager, string>>
            {
                { CommandOptions.software.ToString(), BuySoftware },
                { CommandOptions.component.ToString(), BuyComponent }
            };
        }

        public override void Execute(GameManager game, string command)
        {
            string[] commandComponents = command.Split(new[] { '\'', '"' }, StringSplitOptions.RemoveEmptyEntries);
            ConsoleText console = game.Console;

            if (commandComponents.Length > 2)
            {
                console.AddMessage($"The buy command requires to specify something to buy and only one", MessageType.Warning);
                return;
            }

            string[] commandParameters = commandComponents[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commandParameters.Length < 2 || commandParameters.Length > 3)
            {
                console.AddMessage($"The buy command requires to specify the type of things you want to buy", MessageType.Warning);
                return;
            }

            if (!buyOptions.ContainsKey(commandParameters[1]))
            {
                console.AddMessage($"The buy option is not available", MessageType.Warning);
                return;
            }

            string product = commandComponents.Length == 2 ? commandComponents[1] : commandParameters[2];

            buyOptions[commandParameters[1]](game, product);
        }

        private void BuyComponent(GameManager game, string componentName)
        {
            StoreComponent component = game.GetStoreComponent(componentName);

            if (component == null)
            {
                game.Console.AddMessage($"Component {componentName} not found", MessageType.Error);
                return;
            }

            if (!game.TryBuyComponent(component.Price, component.SoldComponent))
            {
                game.Console.AddMessage("Not enough many!", MessageType.Error);
            }
        }

        private void BuySoftware(GameManager game, string softwareName)
        {
            Software software = game.GetStoreSoftware(softwareName);

            if (software == null)
            {
                game.Console.AddMessage($"Software {softwareName} not found", MessageType.Error);
                return;
            }

            if (!game.TryBuySoftware(software.Price, software.Provides))
            {
                game.Console.AddMessage("Not enough many!", MessageType.Error);
            }
        }
    }
}
