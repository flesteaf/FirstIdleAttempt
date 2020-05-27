using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Commands
{
    internal class ScanCommand : Command
    {
        public override string Name => "scan";

        public override void Execute(GameManager game, string command)
        {
            throw new NotImplementedException();
        }
    }
}
