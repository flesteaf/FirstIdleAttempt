using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Softwares;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class InjectCommand : Command
    {
        public override CommandNames Name => CommandNames.inject;

        private readonly Dictionary<CommandOptions, Action<GameData, string>> injectTypes;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.miner,
                            CommandOptions.bot,
                            CommandOptions.spammer,
                            CommandOptions.ransomware };
        }

        public InjectCommand()
        {
            injectTypes = new Dictionary<CommandOptions, Action<GameData, string>>
            {
                { CommandOptions.miner, InjectMiner },
                { CommandOptions.bot, InjectBot },
                { CommandOptions.spammer, InjectSpammer },
                { CommandOptions.ransomware, InjectRansomware }
            };
        }

        public override void Execute(GameData game, CommandLine command)
        {
            if (!command.HasArgumentAndOption())
            {
                SendMessage("The inject command receives 2 parameters: inject type (bot, miner, spammer, ransomware) and the ip or mac of the device", MessageType.Warning);
                return;
            }

            if (!injectTypes.ContainsKey(command.Option))
            {
                SendMessage($"Wrong option selected. Option {command.Option} is unrecognized", MessageType.Error);
                return;
            }

            injectTypes[command.Option](game, command.Argument);
        }

        #region InjectCommands

        private void InjectRansomware(GameData game, string identifier)
        {
            //TODO: implement this;
            SendMessage("Not implemented yet", MessageType.Warning);
        }

        private void InjectSpammer(GameData game, string identifier)
        {
            //TODO: implement this;
            SendMessage("Not implemented yet", MessageType.Warning);
        }

        private void InjectBot(GameData game, string identifier)
        {
            //TODO: implement this;
            SendMessage("Not implemented yet", MessageType.Warning);
        }

        private void InjectMiner(GameData game, string identifier)
        {
            Device device = game.GetDeviceByIp(identifier);
            if (device == null)
            {
                device = game.GetDeviceByMac(identifier);

                if (device == null)
                {
                    SendMessage($"The provided device {identifier} was not found.", MessageType.Error);
                    return;
                }
            }

            if (!device.CanBeInfected)
            {
                SendMessage($"The provided device {identifier} cannot be infected, probably the firewall is up.", MessageType.Error);
                return;
            }

            device.Infect(InfectionType.Miner);
        }

        #endregion InjectCommands
    }
}