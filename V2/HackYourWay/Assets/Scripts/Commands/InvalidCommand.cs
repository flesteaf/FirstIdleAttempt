using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class InvalidCommand : Command
    {
        public override CommandNames Name => CommandNames.invalid;

        public override List<CommandOptions> Options => new List<CommandOptions> { CommandOptions.None };

        public override IEnumerator Execute(IGameLogic game, CommandLine command)
        {
            SendMessage($"The command is invalid", MessageType.Error);
            yield break;
        }
    }
}