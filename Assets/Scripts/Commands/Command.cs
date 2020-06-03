using System;

namespace Assets.Scripts.Commands
{
    public abstract class Command
    { 
        //TODO: implement command help option and CanExecute
        public abstract string Name { get; }
        //public abstract string Description { get; }
        protected const string HelpOption = "?";
 
        public abstract void Execute(GameManager game, string command);
        //public abstract bool CanExecute(GameManager game, string command);
    }
}