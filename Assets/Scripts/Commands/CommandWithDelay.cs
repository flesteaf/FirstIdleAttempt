using System.Threading;

namespace Assets.Scripts.Commands
{
    public abstract class CommandWithDelay : Command
    {
        protected abstract int BaseExecutionTime { get; }
        
        public override void Execute(IGameData data, CommandLine command)
        {
            int delayTime = GetCommandDelay(data.GetComputerSpeed(), data.GetNetworkSpeed());

            Execute(data, command, delayTime);
        }

        protected abstract int GetCommandDelay(int computerSpeed, long networkSpeed);
        public abstract void Execute(IGameData game, CommandLine command, int delayTime);

        internal protected static void ExecuteDelay(int delay)
        {
            Thread.Sleep(delay);
        }
    }
}
