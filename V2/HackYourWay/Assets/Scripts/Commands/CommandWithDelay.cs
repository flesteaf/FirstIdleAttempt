using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Assets.Scripts.Commands
{
    public abstract class CommandWithDelay : Command
    {
        protected abstract int BaseExecutionTime { get; }

        public override IEnumerator Execute(IGameData data, CommandLine command)
        {
            long delayTime = GetCommandDelay(data.GetComputerSpeed(), data.GetNetworkSpeed());

            data.CommandUnderExecution = true;
            yield return Execute(data, command, delayTime);
            data.CommandUnderExecution = false;
        }

        protected abstract long GetCommandDelay(int computerSpeed, long networkSpeed);
        public abstract IEnumerator Execute(IGameData game, CommandLine command, long delayTime);

        protected internal IEnumerator ExecuteDelay(long delay, Action action)
        {
            yield return ExecuteDelay(delay);
            action();
        }

        protected internal IEnumerator ExecuteDelay<T>(long delay, Action<T> action, T parameter)
        {
            yield return ExecuteDelay(delay);
            action(parameter);
        }

        protected internal IEnumerator ExecuteDelay<T, Y>(long delay, Action<T, Y> action, T parameter1, Y parameter2)
        {
            yield return ExecuteDelay(delay);
            action(parameter1, parameter2);
        }

        private IEnumerator ExecuteDelay(long delay)
        {
            ProgressAction(0);
            Stopwatch watch = Stopwatch.StartNew();
            yield return new WaitUntil(() => watch.ElapsedMilliseconds > delay / 10);
            for (int i = 1; i <= 9; ++i)
            {
                ProgressAction(i * 10);
                watch.Restart();
                yield return new WaitUntil(() => watch.ElapsedMilliseconds > delay / 10);
            }

            ProgressAction(-1);
        }
    }
}
