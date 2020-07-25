using Assets.Scripts.Networks;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class CrackCommand : Command
    {
        private readonly Dictionary<CommandOptions, Action<GameManager, string>> crackTypes;
        public override CommandNames Name => CommandNames.crack;
        public override List<CommandOptions> Options { 
            get => new List<CommandOptions> { 
                            CommandOptions.wep, 
                            CommandOptions.wpa, 
                            CommandOptions.wpa2 }; 
        }

        public CrackCommand()
        {
            crackTypes = new Dictionary<CommandOptions, Action<GameManager, string>>
            {
                { CommandOptions.wep, CrackWep },
                { CommandOptions.wpa, CrackWpa },
                { CommandOptions.wpa2, CrackWpa2 }
            };
        }

        public override void Execute(GameManager game, CommandLine command)
        {
            ConsoleText console = game.SceneManager.Console;

            if (!command.HasArgumentAndOption())
            {
                console.AddMessage("The crack command receives 2 parameters: crack type (wep, wpa, wpa2) and the SSID of the network", MessageType.Warning);
                return;
            }

            if (!crackTypes.ContainsKey(command.Option))
            {
                console.AddMessage($"Wrong option selected. Option {command.Option} is unrecognized", MessageType.Error);
                return;
            }

            crackTypes[command.Option](game, command.Argument);
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
            HackableNetwork network = game.GetNetworkBySSID(ssid);

            if (network == null)
            {
                game.SceneManager.Console.AddMessage($"Unrecognized network {ssid}", MessageType.Error);
                return;
            }

            network.HackNetwork(protection);

            if (network.WasHacked)
            {
                game.SceneManager.Console.AddMessage($"Network {ssid} is now accessible", MessageType.Info);
            }
            else
            {
                game.SceneManager.Console.AddMessage($"Network {ssid} has protection {network.Protection}. Cannot crack it with {protection}.", MessageType.Error);
            }
        }

        #endregion CrackCommands
    }
}