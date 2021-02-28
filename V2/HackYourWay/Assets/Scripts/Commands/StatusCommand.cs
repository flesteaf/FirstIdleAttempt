using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class StatusCommand : Command
    {
        private readonly Dictionary<CommandOptions, Func<IGameData, IEnumerator>> statusTypes;

        public override CommandNames Name => CommandNames.status;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.component,
                            CommandOptions.money };
        }

        public StatusCommand()
        {
            statusTypes = new Dictionary<CommandOptions, Func<IGameData, IEnumerator>>
            {
                { CommandOptions.computer, GetComputerStatus },
                { CommandOptions.money, GetMoneyIncomeStatus }
            };
        }

        public override IEnumerator Execute(IGameData game, CommandLine command)
        {
            if (!command.HasArgumentAndNoOption())
            {
                SendMessage("Status command provides details about either 'computer' or 'money'.", MessageType.Warning);
                SendMessage("Please provide the component for which to receive status, e.g. 'status computer'", MessageType.Warning);
                yield break;
            }

            if (command.HasArgumentAndOption())
            {
                SendMessage("Status command receives only 1 parameter from 'computer' or 'money'", MessageType.Warning);
                yield break;
            }

            if (!statusTypes.ContainsKey(command.ArgumentAsOption()))
            {
                SendMessage("Invalid parameter as input for status command, accepted are 'computer' or 'money'", MessageType.Error);
                yield break;
            }

            yield return statusTypes[command.ArgumentAsOption()](game);
        }

        #region StatusCommands

        private IEnumerator GetMoneyIncomeStatus(IGameData game)
        {
            //TODO: implement it
            yield break;
        }

        private IEnumerator GetComputerStatus(IGameData game)
        {
            string computerDetails = game.Computer.ToString();
            foreach (var item in computerDetails.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                SendMessage(item, MessageType.Info);
            }

            yield break;
        }

        #endregion StatusCommands
    }
}