using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Commands
{
    internal class SetRansomwareCommand : CommandWithDelay
    {
        public override CommandNames Name => CommandNames.setRansomware;

        public override List<CommandOptions> Options => new List<CommandOptions> { CommandOptions.None };

        protected override int BaseExecutionTime => throw new NotImplementedException();

        public override IEnumerator Execute(IGameData game, CommandLine command, int delayTime)
        {
            yield break;
        }

        protected override int GetCommandDelay(int computerSpeed, long networkSpeed)
        {
            throw new NotImplementedException();
        }
    }
}