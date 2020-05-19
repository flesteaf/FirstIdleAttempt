namespace Assets.Scripts.Commands
{
    internal class InvalidCommand : Command
    {
        public override string Name => "invalid";

        public override void Execute(Game game, string command)
        {
            game.Console.AddMessage($"The command {command} is invalid");
        }
    }
}