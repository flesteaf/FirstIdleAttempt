using Assets.Scripts.Software;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class StoreCommand : Command
    {
        private readonly Dictionary<string, Action<GameManager>> storeTypes;
        public override string Name => "store";

        public StoreCommand()
        {
            storeTypes = new Dictionary<string, Action<GameManager>>
            {
                { CommandOptions.software.ToString(), PresentSoftware },
                { CommandOptions.components.ToString(), PresentComponents }
            };
        }

        public override void Execute(GameManager game, string command)
        {
            string[] components = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (components.Length == 1)
            {
                PresentStoreOptions(game);
                return;
            }

            if (components.Length != 2)
            {
                game.Console.AddMessage("Invalid command call, the store command takes 1 parameter at most. Check help command for store", MessageType.Error);
                return;
            }

            if (!storeTypes.ContainsKey(components[1]))
            {
                game.Console.AddMessage("Invalid parameter as input for store command, accepted are 'software' and 'components'", MessageType.Error);
                return;
            }

            storeTypes[components[1]](game);
        }

        #region StoreOptions

        private void PresentComponents(GameManager game)
        {
            IEnumerable<StoreComponent> components = game.GetAllComponents();

            foreach (var item in components)
            {
                string text = $"{item.Component.Name} - ${item.Price} - {item.Component.LoadUsage} Watt - {item.Description}";
                if (item.WasBought)
                {
                    text += " | Bought";
                }

                game.Console.AddMessage(text, MessageType.Info);
            }
        }

        private void PresentSoftware(GameManager game)
        {
            IEnumerable<Software.Software> softwares = game.GetAllSoftwares();

            foreach (var item in softwares)
            {
                string text = $"{item.Name} - ${item.Price} - {item.Description}";
                if (item.WasBought)
                {
                    text += " | Bought";
                }

                game.Console.AddMessage(text, MessageType.Info);
            }
        }
        
        private void PresentStoreOptions(GameManager game)
        {
            game.Console.AddMessage("store software - for buying new software", MessageType.Info);
            game.Console.AddMessage("store components - for upgrading your computer", MessageType.Info);
        }

        #endregion StoreOptions
    }
}
