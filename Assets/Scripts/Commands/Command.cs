using System;

namespace Assets.Scripts.Commands
{
    public abstract class Command
    {
        public abstract String Name { get; }

        public abstract void Execute(GameManager game, string command);
    }
}