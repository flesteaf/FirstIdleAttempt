using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Commands
{
    public class ClearCommand : Command
    {
        public override CommandNames Name => CommandNames.clear;

        public override List<CommandOptions> Options => new List<CommandOptions> { CommandOptions.None };

        public override IEnumerator Execute(IGameData game, CommandLine command)
        {
            game.ClearConsole();
            yield break;
        }
    }
}
