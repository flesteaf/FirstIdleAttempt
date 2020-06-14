using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class StatusCommand : Command
    {
        private readonly Dictionary<string, Action<GameManager>> statusTypes;

        public override string Name => CommandNames.status.ToString();

        public StatusCommand()
        {
            statusTypes = new Dictionary<string, Action<GameManager>>
            {
                { CommandOptions.computer.ToString(), GetComputerStatus },
                { CommandOptions.money.ToString(), GetMoneyIncomeStatus }
            };
        }

        public override void Execute(GameManager game, string command)
        {
            ConsoleText console = game.Console;
            var commandComponents = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commandComponents.Length < 2)
            {
                console.AddMessage("Status command provides details about either 'computer' or 'money'.", MessageType.Warning);
                console.AddMessage("Please provide the component for which to receive status, e.g. 'status computer'", MessageType.Warning);
                return;
            }

            if (commandComponents.Length > 2)
            {
                console.AddMessage("Status command receives only 1 parameter from 'computer' or 'money'", MessageType.Warning);
                return;
            }

            if (!statusTypes.ContainsKey(commandComponents[1]))
            {
                game.Console.AddMessage("Invalid parameter as input for status command, accepted are 'computer' or 'money'", MessageType.Error);
                return;
            }

            statusTypes[commandComponents[1]](game);
        }

        #region StatusCommands

        private void GetMoneyIncomeStatus(GameManager game)
        {
            //TODO: implement it
            return;
        }

        private void GetComputerStatus(GameManager game)
        {
            string computerDetails = game.Computer.ToString();
            foreach (var item in computerDetails.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                game.Console.AddMessage(item, MessageType.Info);
            }
        }

        #endregion StatusCommands
    }
}