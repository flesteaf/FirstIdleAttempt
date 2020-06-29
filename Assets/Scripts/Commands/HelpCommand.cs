using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class HelpCommand : Command
    {
        public override CommandNames Name => CommandNames.help;

        public override List<CommandOptions> Options => new List<CommandOptions> { CommandOptions.None };

        public override void Execute(GameManager game, string command)
        {
            game.Console.AddMessage("The following commands are currently allowed:", MessageType.Info);

            var commands = CommandFactory.GetAllCommandsName();
            foreach (string item in commands)
            {
                game.Console.AddMessage($"   - {item}", MessageType.Info);
            }
        }
    }
}