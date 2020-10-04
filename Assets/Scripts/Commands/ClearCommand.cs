using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Commands
{
    public class ClearCommand : Command
    {
        public override CommandNames Name => CommandNames.clear;

        public override List<CommandOptions> Options => new List<CommandOptions> { CommandOptions.None };

        public override void Execute(IGameData game, CommandLine command)
        {
            game.ClearConsole();
        }
    }
}
