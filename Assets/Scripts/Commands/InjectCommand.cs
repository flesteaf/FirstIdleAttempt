using Assets.Scripts.Computers.ComponentTypes;
using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Softwares;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class InjectCommand : CommandWithDelay
    {
        private int delayExecutionTime;
        private readonly Dictionary<CommandOptions, Func<IGameData, string, IEnumerator>> injectTypes;
        private readonly long networkCommunication = 10 * (long)Sizes.MB;

        public override CommandNames Name => CommandNames.inject;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.miner,
                            CommandOptions.bot,
                            CommandOptions.spammer,
                            CommandOptions.ransomware };
        }

        protected override int BaseExecutionTime => 10000;

        public InjectCommand()
        {
            injectTypes = new Dictionary<CommandOptions, Func<IGameData, string, IEnumerator>>
            {
                { CommandOptions.miner, InjectMiner },
                { CommandOptions.bot, InjectBot },
                { CommandOptions.spammer, InjectSpammer },
                { CommandOptions.ransomware, InjectRansomware }
            };
        }

        public override IEnumerator Execute(IGameData game, CommandLine command, int delayTime)
        {
            delayExecutionTime = delayTime;
            if (!command.HasArgumentAndOption())
            {
                SendMessage("The inject command receives 2 parameters: inject type (bot, miner, spammer, ransomware) and the ip or mac of the device", MessageType.Warning);
                yield break;
            }

            if (!injectTypes.ContainsKey(command.Option))
            {
                SendMessage($"Wrong option selected. Option {command.Option} is unrecognized", MessageType.Error);
                yield break;
            }

            yield return injectTypes[command.Option](game, command.Argument);
        }

        #region InjectCommands

        private IEnumerator InjectRansomware(IGameData game, string identifier)
        {
            //TODO: implement this;
            SendMessage("Not implemented yet", MessageType.Warning);
            yield break;
        }

        private IEnumerator InjectSpammer(IGameData game, string identifier)
        {
            //TODO: implement this;
            SendMessage("Not implemented yet", MessageType.Warning);
            yield break;
        }

        private IEnumerator InjectBot(IGameData game, string identifier)
        {
            //TODO: implement this;
            SendMessage("Not implemented yet", MessageType.Warning);
            yield break;
        }

        private IEnumerator InjectMiner(IGameData game, string identifier)
        {
            Device device = game.GetDeviceByIp(identifier);
            if (device == null)
            {
                device = game.GetDeviceByMac(identifier);

                if (device == null)
                {
                    SendMessage($"The provided device {identifier} was not found.", MessageType.Error);
                    yield break;
                }
            }

            if (!device.CanBeInfected)
            {
                SendMessage($"The provided device {identifier} cannot be infected, probably the firewall is up.", MessageType.Error);
                yield break;
            }

            yield return ExecuteDelay(delayExecutionTime, device.Infect, InfectionType.Miner);
        }

        protected override int GetCommandDelay(int computerSpeed, long networkSpeed)
        {
            return BaseExecutionTime / computerSpeed + (int)(networkCommunication / networkSpeed);
        }

        #endregion InjectCommands
    }
}