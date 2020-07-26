using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    public class BuyCommand : Command
    {
        private readonly Dictionary<CommandOptions, Action<SceneManager, string>> buyOptions;
        public override CommandNames Name => CommandNames.buy;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.software,
                            CommandOptions.component };
        }

        public BuyCommand()
        {
            buyOptions = new Dictionary<CommandOptions, Action<SceneManager, string>>
            {
                { CommandOptions.software, BuySoftware },
                { CommandOptions.component, BuyComponent }
            };
        }

        public override void Execute(SceneManager game, CommandLine command)
        {
            IConsoleText console = game.Console;

            if (!command.LongArgument)
            {
                console.AddMessage($"The buy command requires to specify something to buy and only one", MessageType.Warning);
                return;
            }

            if (command.Option == CommandOptions.None)
            {
                console.AddMessage($"The buy command requires to specify the type of things you want to buy", MessageType.Warning);
                return;
            }

            if (!buyOptions.ContainsKey(command.Option))
            {
                console.AddMessage($"The buy option is not available", MessageType.Warning);
                return;
            }

            buyOptions[command.Option](game, command.Argument);
        }

        private void BuyComponent(SceneManager game, string componentName)
        {
            StoreComponent component = game.GetStoreComponent(componentName);

            if (component == null)
            {
                game.Console.AddMessage($"Component {componentName} not found", MessageType.Error);
                return;
            }

            if (!game.TryBuyComponent(component))
            {
                game.Console.AddMessage("Not enough many!", MessageType.Error);
            }
        }

        private void BuySoftware(SceneManager game, string softwareName)
        {
            Software software = game.GetStoreSoftware(softwareName);

            if (software == null)
            {
                game.Console.AddMessage($"Software {softwareName} not found", MessageType.Error);
                return;
            }

            if (!game.TryBuySoftware(software))
            {
                game.Console.AddMessage("Not enough money!", MessageType.Error);
            }
        }
    }
}
