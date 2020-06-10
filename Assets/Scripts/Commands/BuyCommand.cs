using System;

namespace Assets.Scripts.Commands
{
    public class BuyCommand : Command
    {
        public override string Name => "buy";

        public override void Execute(GameManager game, string command)
        {
            string[] commandComponents = command.Split(new[] { '\'', '"' });

        }
    }
}
