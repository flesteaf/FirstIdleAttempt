using Assets.Scripts.Networks.Devices;
using Assets.Scripts.Software;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class InjectCommand : Command
    {
        public override string Name => "inject";

        private readonly Dictionary<string, Action<GameManager, string>> injectTypes;

        public InjectCommand()
        {
            injectTypes = new Dictionary<string, Action<GameManager, string>>
            {
                { CommandOptions.miner.ToString(), InjectMiner },
                { CommandOptions.bot.ToString(), InjectBot },
                { CommandOptions.spammer.ToString(), InjectSpammer },
                { CommandOptions.ransomware.ToString(), InjectRansomware }
            }; 
        }

        public override void Execute(GameManager game, string command)
        {
            ConsoleText console = game.Console;
            var commandComponents = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (commandComponents.Length != 3)
            {
                console.AddMessage("The inject command receives 2 parameters: inject type (bot, miner, spammer, ransomware) and the ip or mac of the device", MessageType.Warning);
                return;
            }

            if (!injectTypes.ContainsKey(commandComponents[1]))
            {
                console.AddMessage($"Wrong option selected. Option {commandComponents[1]} is unrecognized", MessageType.Error);
                return;
            }

            injectTypes[commandComponents[1]](game, commandComponents[2]);
        }

        #region InjectCommands

        private void InjectRansomware(GameManager game, string identifier)
        {
            //TODO: implement this;
            game.Console.AddMessage("Not implemented yet", MessageType.Warning);
        }

        private void InjectSpammer(GameManager game, string identifier)
        {
            //TODO: implement this;
            game.Console.AddMessage("Not implemented yet", MessageType.Warning);
        }

        private void InjectBot(GameManager game, string identifier)
        {
            //TODO: implement this;
            game.Console.AddMessage("Not implemented yet", MessageType.Warning);
        }

        private void InjectMiner(GameManager game, string identifier)
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