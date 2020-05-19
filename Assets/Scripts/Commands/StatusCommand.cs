using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Commands
{
    internal class StatusCommand : Command
    {
        private readonly Dictionary<string, Action<Game>> statusTypes;

        public override string Name => "status";

        public StatusCommand()
        {
            statusTypes = new Dictionary<string, Action<Game>>
            {
                { "computer", GetComputerStatus },
                { "money", GetMoneyIncomeStatus }
            };
        }

        public override void Execute(Game game, string command)
        {
            ConsoleText console = game.Console;
            var commandComponents = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commandComponents.Length < 2)
            {
                console.AddMessage("Status command provides details about either 'computer' or 'money'.");
                console.AddMessage("Please provide the component for which to receive status, e.g. 'status computer'");
                return;
            }

            if (commandComponents.Length > 2)
            {
                console.AddMessage("Status command receives only 1 parameter from 'computer' or 'money'");
                return;
            }

            HandleStatus(game, commandComponents[1]);
        }

        private void HandleStatus(Game game, string parameter)
        {
            if (!statusTypes.ContainsKey(parameter))
            {
                game.Console.AddMessage("Invalid parameter as input for status command, accepted are 'computer' or 'money'");
                return;
            }

            statusTypes[parameter](game);
        }

        #region StatusCommands
        private void GetMoneyIncomeStatus(Game game)
        {
            //TODO: implement it
            return;
        }

        private void GetComputerStatus(Game game)
        {
            string computerDetails = game.Computer.ToString();
            foreach (var item in computerDetails.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                game.Console.AddMessage(item);
            }
        }
        #endregion
    }
}