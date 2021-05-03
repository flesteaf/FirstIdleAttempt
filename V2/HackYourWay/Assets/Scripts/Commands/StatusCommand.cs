using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class StatusCommand : Command
    {
        private readonly Dictionary<CommandOptions, Func<IGameLogic, IEnumerator>> statusTypes;

        public override CommandNames Name => CommandNames.status;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> {
                            CommandOptions.component,
                            CommandOptions.money };
        }

        public StatusCommand()
        {
            statusTypes = new Dictionary<CommandOptions, Func<IGameLogic, IEnumerator>>
            {
                { CommandOptions.computer, GetComputerStatus },
                { CommandOptions.money, GetMoneyIncomeStatus }
            };
        }

        public override IEnumerator Execute(IGameLogic game, CommandLine command)
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

        private IEnumerator GetMoneyIncomeStatus(IGameLogic game)
        {
            //TODO: implement it
            yield break;
        }

        private IEnumerator GetComputerStatus(IGameLogic game)
        {
            string computerDetails = game.PlayerData.Computer.ToString();
            foreach (var item in computerDetails.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                SendMessage(item, MessageType.Info);
            }

            yield break;
        }

        #endregion StatusCommands
    }
}