using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Commands
{
    internal class HelpCommand : Command
    {
        public override CommandNames Name => CommandNames.help;

        public override List<CommandOptions> Options => new List<CommandOptions> { CommandOptions.None };

        public override void Execute(GameManager game, CommandLine command)
        {
            //TODO: improve to provide help for all commands
            game.SceneManager.Console.AddMessage("The following commands are currently allowed:", MessageType.Info);

            var commands = CommandFactory.GetAllCommandsName();
            foreach (string item in commands)
            {
                game.SceneManager.Console.AddMessage($"   - {item}", MessageType.Info);
            }
        }
    }
}