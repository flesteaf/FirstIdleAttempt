using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Commands
{
    internal class SetRansomwareCommand : Command
    {
        public override string Name => "set-ransomware";

        public override void Execute(GameManager game, string command)
        {
            throw new NotImplementedException();
        }
    }
}
