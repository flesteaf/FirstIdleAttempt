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

        private readonly Dictionary<CommandOptions, Action<SceneManager, string>> injectTypes;
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
            injectTypes = new Dictionary<CommandOptions, Action<SceneManager, string>>
            {
                { CommandOptions.miner, InjectMiner },
                { CommandOptions.bot, InjectBot },
                { CommandOptions.spammer, InjectSpammer },
                { CommandOptions.ransomware, InjectRansomware }
            };
        }

        public override void Execute(SceneManager game, CommandLine command)
        {
            IConsoleText console = game.Console;

            if (!command.HasArgumentAndOption())
            {
                console.AddMessage("The inject command receives 2 parameters: inject type (bot, miner, spammer, ransomware) and the ip or mac of the device", MessageType.Warning);
                return;
            }

            if (!injectTypes.ContainsKey(command.Option))
            {
                console.AddMessage($"Wrong option selected. Option {command.Option} is unrecognized", MessageType.Error);
                return;
            }

            injectTypes[command.Option](game, command.Argument);
        }

        #region InjectCommands

        private void InjectRansomware(SceneManager game, string identifier)
        {
            //TODO: implement this;
            game.Console.AddMessage("Not implemented yet", MessageType.Warning);
        }

        private void InjectSpammer(SceneManager game, string identifier)
        {
            //TODO: implement this;
            game.Console.AddMessage("Not implemented yet", MessageType.Warning);
        }

        private void InjectBot(SceneManager game, string identifier)
        {
            //TODO: implement this;
            game.Console.AddMessage("Not implemented yet", MessageType.Warning);
        }

        private void InjectMiner(SceneManager game, string identifier)
        {
            Device device = game.GetDeviceByIp(identifier);
            if (device == null)
            {
                device = game.GetDeviceByMac(identifier);

                if (device == null)
                {
                    game.Console.AddMessage($"The provided device {identifier} was not found.", MessageType.Error);
                    return;
                }
            }

            if (!device.CanBeInfected)
            {
                game.Console.AddMessage($"The provided device {identifier} cannot be infected, probably the firewall is up.", MessageType.Error);
                return;
            }

            device.Infect(InfectionType.Miner);
        }

        #endregion InjectCommands
    }
}