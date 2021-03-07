using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Commands
{
    internal class SetRansomwareCommand : Command
    {
        public override CommandNames Name => CommandNames.setRansomware;

        public override List<CommandOptions> Options => new List<CommandOptions> { CommandOptions.None };

        public override IEnumerator Execute(IGameData data, CommandLine command)
        {
            //TODO: implement this
            yield break;
        }
    }
}