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
        private readonly Dictionary<CommandOptions, Action<GameData, string>> buyOptions;
        public override CommandNames Name => CommandNames.buy;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.software,
                            CommandOptions.component };
        }

        public BuyCommand()
        {
            buyOptions = new Dictionary<CommandOptions, Action<GameData, string>>
            {
                { CommandOptions.software, BuySoftware },
                { CommandOptions.component, BuyComponent }
            };
        }

        public override void Execute(GameData game, CommandLine command)
        {
            if (!command.LongArgument)
            {
                SendMessage($"The buy command requires to specify something to buy and only one", MessageType.Warning);
                return;
            }

            if (command.Option == CommandOptions.None)
            {
                SendMessage($"The buy command requires to specify the type of things you want to buy", MessageType.Warning);
                return;
            }

            if (!buyOptions.ContainsKey(command.Option))
            {
                SendMessage($"The buy option is not available", MessageType.Warning);
                return;
            }

            buyOptions[command.Option](game, command.Argument);
        }

        private void BuyComponent(GameData game, string componentName)
        {
            StoreComponent component = game.Store.GetComponent(componentName);

            if (component == null)
            {
                SendMessage($"Component {componentName} not found", MessageType.Error);
                return;
            }

            if (!game.TryBuyComponent(component))
            {
                SendMessage("Not enough many!", MessageType.Error);
            }
        }

        private void BuySoftware(GameData game, string softwareName)
        {
            Software software = game.Store.GetSoftware(softwareName);

            if (software == null)
            {
                SendMessage($"Software {softwareName} not found", MessageType.Error);
                return;
            }

            if (!game.TryBuySoftware(software))
            {
                SendMessage("Not enough money!", MessageType.Error);
            }
        }
    }
}
