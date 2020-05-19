using System.Collections.Generic;

namespace Assets.Scripts.Commands
{
    internal class HelpCommand : Command
    {
        public override string Name { get => "help"; }

        public override void Execute(Game game, string command)
        {
            game.Console.AddMessage("The following commands are currently allowed:");
            
            var commands = CommandFactory.GetAllCommandsName();
            foreach (string item in commands)
            {
                game.Console.AddMessage($"   - {item}");
            }
        }
    }
}