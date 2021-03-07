using Assets.Scripts.Computers.ComponentTypes;
using Assets.Scripts.Networks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class CrackCommand : CommandWithDelay
    {
        private readonly long NetworkDataForCrack = (long)Sizes.MB;
        private readonly Dictionary<CommandOptions, Func<IGameData, string, IEnumerator>> crackTypes;
        private long delayExecutionTime;
        public override CommandNames Name => CommandNames.crack;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.wep,
                            CommandOptions.wpa,
                            CommandOptions.wpa2 };
        }

        protected override int BaseExecutionTime => 4000;

        public CrackCommand()
        {
            crackTypes = new Dictionary<CommandOptions, Func<IGameData, string, IEnumerator>>
            {
                { CommandOptions.wep, CrackWep },
                { CommandOptions.wpa, CrackWpa },
                { CommandOptions.wpa2, CrackWpa2 }
            };
        }

        public override IEnumerator Execute(IGameData game, CommandLine command, long delayTime)
        {
            delayExecutionTime = delayTime;
            if (!command.HasArgumentAndOption())
            {
                SendMessage("The crack command receives 2 parameters: crack type (wep, wpa, wpa2) and the SSID of the network", MessageType.Warning);
                yield break;
            }

            if (!crackTypes.ContainsKey(command.Option))
            {
                SendMessage($"Wrong option selected. Option {command.Option} is unrecognized", MessageType.Error);
                yield break;
            }

            yield return crackTypes[command.Option](game, command.Argument);
        }

        #region CrackCommands

        private IEnumerator CrackWpa2(IGameData game, string ssid)
        {
            yield return CrackNetwork(game, ssid, ProtectionType.WPA2);
        }

        private IEnumerator CrackWpa(IGameData game, string ssid)
        {
            yield return CrackNetwork(game, ssid, ProtectionType.WPA);
        }

        private IEnumerator CrackWep(IGameData game, string ssid)
        {
            yield return CrackNetwork(game, ssid, ProtectionType.WEP);
        }

        private IEnumerator CrackNetwork(IGameData game, string ssid, ProtectionType protection)
        {
            HackableNetwork network = game.GetNetworkBySSID(ssid);

            if (network == null)
            {
                SendMessage($"Unrecognized network {ssid}", MessageType.Error);
                yield break;
            }

            yield return ExecuteDelay(delayExecutionTime + ((int)protection * 1000), network.HackNetwork, protection);

            if (network.WasHacked)
            {
                SendMessage($"Network {ssid} is now accessible", MessageType.Info);
            }
            else
            {
                SendMessage($"Network {ssid} has protection {network.Protection}. Cannot crack it with {protection}.", MessageType.Error);
            }
            yield break;
        }

        #endregion CrackCommands

        protected override long GetCommandDelay(int computerSpeed, long networkSpeed)
        {
            return BaseExecutionTime / computerSpeed + NetworkDataForCrack / networkSpeed;
        }
    }
}