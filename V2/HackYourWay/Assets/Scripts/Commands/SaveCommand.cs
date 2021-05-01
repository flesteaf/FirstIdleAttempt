using Assets.Scripts.Softwares;
using Assets.Scripts.Store;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Commands
{
    public class SaveCommand : Command
    {
        private readonly Dictionary<CommandOptions, Action<IGameLogic, string>> saveOptions;
        public override CommandNames Name => CommandNames.save;
        public override List<CommandOptions> Options
        {
            get => new List<CommandOptions> { CommandOptions.clear };
        }

        public SaveCommand()
        {
            saveOptions = new Dictionary<CommandOptions, Action<IGameLogic, string>>
            {
                { CommandOptions.clear, ClearSave },
                { CommandOptions.None, SaveGame },
                //{ CommandOptions.showSlots, ShowSaveSlots }
            };
        }

        public override IEnumerator Execute(IGameLogic game, CommandLine command)
        {
            if (command.TooManyArguments)
            {
                SendMessage($"The save command requires to specify a maximum of one save slot to handle", MessageType.Warning);
                yield break;
            }

            if (!saveOptions.ContainsKey(command.Option))
            {
                SendMessage($"The buy option is not available", MessageType.Warning);
                yield break;
            }

            saveOptions[command.Option](game, command.Argument);
            yield break;
        }

        private void SaveGame(IGameLogic game, string slot)
        {
            //if (!string.IsNullOrEmpty(slot))
            //{
            //    game.ChangeSaveSlot(slot);
            //}

            game.SavePlayerToCurrentSlot();
        }

        private void ClearSave(IGameLogic game, string slot)
        {
            //if (!string.IsNullOrEmpty(slot))
            //{
            //    game.ChangeSaveSlot(slot);
            //}

            game.ClearSaveSlot();
        }
    }
}
