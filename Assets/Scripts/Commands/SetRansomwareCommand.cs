﻿using System;
using System.Collections.Generic;

namespace Assets.Scripts.Commands
{
    internal class SetRansomwareCommand : Command
    {
        public override CommandNames Name => CommandNames.setRansomware;

        public override List<CommandOptions> Options => new List<CommandOptions> { CommandOptions.None };

        public override void Execute(IGameData game, CommandLine command)
        {
            throw new NotImplementedException();
        }
    }
}