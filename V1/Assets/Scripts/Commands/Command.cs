﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using static Assets.Scripts.HackerDelegates;

namespace Assets.Scripts.Commands
{
    public abstract class Command
    {
        //TODO: implement command help option and CanExecute
        public abstract CommandNames Name { get; }
        public abstract List<CommandOptions> Options { get; }
        public event SendMessageEventHandler MessageNotification;
        public event ActionProgressEventHandler ActionProgress;

        //public abstract string Description { get; }
        protected const string HelpOption = "?";

        public abstract IEnumerator Execute(IGameData data, CommandLine command);

        public CommandOptions GetOptionFromCommand(CommandLine command)
        {
            if (command.Option == CommandOptions.None)
            {
                return command.Option;
            }

            if (!Options.Contains(command.Option))
            {
                return CommandOptions.Invalid;
            }

            return command.Option;
        }

        protected void SendMessage(string message, MessageType type)
        {
            MessageNotification?.Invoke(message, type);
        }

        protected void ProgressAction(int progress)
        {
            ActionProgress?.Invoke(progress);
        }
    }
}