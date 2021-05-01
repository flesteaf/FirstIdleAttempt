using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Commands
{
    public class BuyCommand : Command
    {
        private readonly Dictionary<CommandOptions, Action<IGameLogic, string>> buyOptions;
        private readonly GameStore store;
        public override CommandNames Name => CommandNames.buy;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.software,
                            CommandOptions.component };
        }

        public BuyCommand(GameStore store)
        {
            this.store = store;
            buyOptions = new Dictionary<CommandOptions, Action<IGameLogic, string>>
            {
                { CommandOptions.software, BuySoftware },
                { CommandOptions.component, BuyComponent }
            };
        }

        public override IEnumerator Execute(IGameLogic game, CommandLine command)
        {
            if (!command.TooManyArguments)
            {
                SendMessage($"The buy command requires to specify something to buy and only one", MessageType.Warning);
                yield break;
            }

            if (command.Option == CommandOptions.None)
            {
                SendMessage($"The buy command requires to specify the type of things you want to buy", MessageType.Warning);
                yield break;
            }

            if (!buyOptions.ContainsKey(command.Option))
            {
                SendMessage($"The buy option is not available", MessageType.Warning);
                yield break;
            }

            buyOptions[command.Option](game, command.Argument);
            yield break;
        }

        private void BuyComponent(IGameLogic game, string componentName)
        {
            StoreComponent component = store.GetComponent(componentName);

            if (component == null)
            {
                SendMessage($"Component \"{componentName}\" not found", MessageType.Error);
                return;
            }

            if (!game.TryBuyComponent(component, out string message))
            {
                SendMessage(message, MessageType.Error);
            }
            else
            {
                store.ComponentBought(component);
                SendMessage($"Component \"{componentName}\" successfuly bought!", MessageType.Info);
            }
        }

        private void BuySoftware(IGameLogic game, string softwareName)
        {
            Software software = store.GetSoftware(softwareName);

            if (software == null)
            {
                SendMessage($"Software \"{softwareName}\" not found", MessageType.Error);
                return;
            }

            if (!game.TryBuySoftware(software, out string message))
            {
                SendMessage(message, MessageType.Error);
            }
            else
            {
                store.SoftwareBought(software);
                SendMessage($"Software \"{softwareName}\" successfuly bought!", MessageType.Info);
            }
        }
    }
}
