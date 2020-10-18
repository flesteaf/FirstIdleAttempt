using Assets.Scripts.Computers.ComponentTypes;
using Assets.Scripts.Networks;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class CrackCommand : CommandWithDelay
    {
        private static long NetworkDataForCrack = 10*(long)Sizes.MB;
        private readonly Dictionary<CommandOptions, Action<IGameData, string>> crackTypes;
        private int delayExecutionTime;
        public override CommandNames Name => CommandNames.crack;
        public override List<CommandOptions> Options { 
            get => new List<CommandOptions> { 
                            CommandOptions.wep, 
                            CommandOptions.wpa, 
                            CommandOptions.wpa2 }; 
        }

        protected override int BaseExecutionTime => 100000;

        public CrackCommand()
        {
            crackTypes = new Dictionary<CommandOptions, Action<IGameData, string>>
            {
                { CommandOptions.wep, CrackWep },
                { CommandOptions.wpa, CrackWpa },
                { CommandOptions.wpa2, CrackWpa2 }
            };
        }

        public override void Execute(IGameData game, CommandLine command, int delayTime)
        {
            delayExecutionTime = delayTime;
            if (!command.HasArgumentAndOption())
            {
                SendMessage("The crack command receives 2 parameters: crack type (wep, wpa, wpa2) and the SSID of the network", MessageType.Warning);
                return;
            }

            if (!crackTypes.ContainsKey(command.Option))
            {
                SendMessage($"Wrong option selected. Option {command.Option} is unrecognized", MessageType.Error);
                return;
            }

            crackTypes[command.Option](game, command.Argument);
        }

        #region CrackCommands

        private void CrackWpa2(IGameData game, string ssid)
        {
            CrackNetwork(game, ssid, ProtectionType.WPA2);
        }

        private void CrackWpa(IGameData game, string ssid)
        {
            CrackNetwork(game, ssid, ProtectionType.WPA);
        }

        private void CrackWep(IGameData game, string ssid)
        {
            CrackNetwork(game, ssid, ProtectionType.WEP);
        }

        private void CrackNetwork(IGameData game, string ssid, ProtectionType protection)
        {
            HackableNetwork network = game.GetNetworkBySSID(ssid);

            if (network == null)
            {
                SendMessage($"Unrecognized network {ssid}", MessageType.Error);
                return;
            }

            ExecuteDelay(delayExecutionTime + ((int)protection*1000));
            network.HackNetwork(protection);

            if (network.WasHacked)
            {
                SendMessage($"Network {ssid} is now accessible", MessageType.Info);
            }
            else
            {
                SendMessage($"Network {ssid} has protection {network.Protection}. Cannot crack it with {protection}.", MessageType.Error);
            }
        }

        #endregion CrackCommands

        protected override int GetCommandDelay(int computerSpeed, long networkSpeed)
        {
            return BaseExecutionTime / computerSpeed + (int)(NetworkDataForCrack / networkSpeed);
        }
    }
}