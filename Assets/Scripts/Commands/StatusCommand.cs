using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class StatusCommand : Command
    {
        private readonly Dictionary<CommandOptions, Action<GameManager>> statusTypes;

        public override CommandNames Name => CommandNames.status;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.component,
                            CommandOptions.money };
        }

        public StatusCommand()
        {
            statusTypes = new Dictionary<CommandOptions, Action<GameManager>>
            {
                { CommandOptions.computer, GetComputerStatus },
                { CommandOptions.money, GetMoneyIncomeStatus }
            };
        }

        public override void Execute(GameManager game, CommandLine command)
        {
            ConsoleText console = game.SceneManager.Console;
            if (!command.HasArgumentAndNoOption())
            {
                console.AddMessage("Status command provides details about either 'computer' or 'money'.", MessageType.Warning);
                console.AddMessage("Please provide the component for which to receive status, e.g. 'status computer'", MessageType.Warning);
                return;
            }

            if (command.HasArgumentAndOption())
            {
                console.AddMessage("Status command receives only 1 parameter from 'computer' or 'money'", MessageType.Warning);
                return;
            }

            if (!statusTypes.ContainsKey(command.ArgumentAsOption()))
            {
                console.AddMessage("Invalid parameter as input for status command, accepted are 'computer' or 'money'", MessageType.Error);
                return;
            }

            statusTypes[command.ArgumentAsOption()](game);
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
                game.SceneManager.Console.AddMessage(item, MessageType.Info);
            }
        }

        #endregion StatusCommands
    }
}