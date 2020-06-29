using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class InvalidCommand : Command
    {
        public override CommandNames Name => CommandNames.invalid;

        public override List<CommandOptions> Options => new List<CommandOptions> { CommandOptions.None };

        public override void Execute(GameManager game, string command)
        {
            game.Console.AddMessage($"The command {command} is invalid", MessageType.Error);
        }
    }
}