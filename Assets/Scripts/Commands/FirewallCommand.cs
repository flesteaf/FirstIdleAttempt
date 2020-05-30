using System;

namespace Assets.Scripts.Commands
{
    internal class FirewallCommand : Command
    {
        public override string Name => "firewall";

        public override void Execute(GameManager game, string command)
        {
            throw new NotImplementedException();
        }
    }
}