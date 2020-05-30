﻿using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class StatusCommand : Command
    {
        private readonly Dictionary<string, Action<GameManager>> statusTypes;

        public override string Name => "status";

        public StatusCommand()
        {
            statusTypes = new Dictionary<string, Action<GameManager>>
            {
                { "computer", GetComputerStatus },
                { "money", GetMoneyIncomeStatus }
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

            HandleStatus(game, commandComponents[1]);
        }

        private void HandleStatus(GameManager game, string parameter)
        {
            if (!statusTypes.ContainsKey(parameter))
            {
                game.Console.AddMessage("Invalid parameter as input for status command, accepted are 'computer' or 'money'", MessageType.Error);
                return;
            }

            statusTypes[parameter](game);
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