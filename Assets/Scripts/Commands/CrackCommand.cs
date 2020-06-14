using Assets.Scripts.Networks;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class CrackCommand : Command
    {
        private readonly Dictionary<string, Action<GameManager, string>> crackTypes;
        public override string Name => CommandNames.crack.ToString();

        public CrackCommand()
        {
            crackTypes = new Dictionary<string, Action<GameManager, string>>
            {
                { CommandOptions.wep.ToString(), CrackWep },
                { CommandOptions.wpa.ToString(), CrackWpa },
                { CommandOptions.wpa2.ToString(), CrackWpa2 }
            };
        }

        public override void Execute(GameManager game, string command)
        {
            ConsoleText console = game.Console;
            var commandComponents = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (commandComponents.Length != 3)
            {
                console.AddMessage("The crack command receives 2 parameters: crack type (wep, wpa, wpa2) and the SSID of the network", MessageType.Warning);
                return;
            }

            if (!crackTypes.ContainsKey(commandComponents[1]))
            {
                console.AddMessage($"Wrong option selected. Option {commandComponents[1]} is unrecognized", MessageType.Error);
                return;
            }

            crackTypes[commandComponents[1]](game, commandComponents[2]);
        }

        #region CrackCommands

        private void CrackWpa2(GameManager game, string ssid)
        {
            CrackNetwork(game, ssid, ProtectionType.WPA2);
        }

        private void CrackWpa(GameManager game, string ssid)
        {
            CrackNetwork(game, ssid, ProtectionType.WPA);
        }

        private void CrackWep(GameManager game, string ssid)
        {
            CrackNetwork(game, ssid, ProtectionType.WEP);
        }

        private static void CrackNetwork(GameManager game, string ssid, ProtectionType protection)
        {
            HackableNetwork network = game.GetNetwork(ssid);

            if (network == null)
            {
                game.Console.AddMessage($"Unrecognized network {ssid}", MessageType.Error);
                return;
            }

            network.HackNetwork(protection);

            if (network.WasHacked)
            {
                game.Console.AddMessage($"Network {ssid} is now accessible", MessageType.Info);
            }
            else
            {
                game.Console.AddMessage($"Network {ssid} has protection {network.Protection}. Cannot crack it with {protection}.", MessageType.Error);
            }
        }

        #endregion CrackCommands
    }
}