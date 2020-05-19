using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Commands
{
    public abstract class Command
    {
        public abstract String Name { get; }

        public abstract void Execute(Game game, string command);
    }
}
