using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Commands
{
    public abstract class CommandWithDelay : Command
    {
        protected abstract int BaseExecutionTime { get; }
        
        public override IEnumerator Execute(IGameData data, CommandLine command)
        {
            int delayTime = GetCommandDelay(data.GetComputerSpeed(), data.GetNetworkSpeed());
            
            data.CommandUnderExecution = true;
            yield return Execute(data, command, delayTime);
            data.CommandUnderExecution = false;
        }

        protected abstract int GetCommandDelay(int computerSpeed, long networkSpeed);
        public abstract IEnumerator Execute(IGameData game, CommandLine command, int delayTime);

        protected internal IEnumerator ExecuteDelay(int delay, Action action)
        {
            yield return ExecuteDelay(delay);
            action();
        }

        protected internal IEnumerator ExecuteDelay<T>(int delay, Action<T> action, T parameter)
        {
            yield return ExecuteDelay(delay);
            action(parameter);
        }

        protected internal IEnumerator ExecuteDelay<T, Y>(int delay, Action<T, Y> action, T parameter1, Y parameter2)
        {
            yield return ExecuteDelay(delay);
            action(parameter1, parameter2);
        }

        private IEnumerator ExecuteDelay(int delay)
        {
            ProgressAction(0);
            yield return new WaitForSecondsRealtime((float)delay / 10000);
            for (int i = 0; i < 10; ++i)
            {
                ProgressAction(i * 10);
                yield return new WaitForSecondsRealtime((float)delay / 10000);
            }

            ProgressAction(-1);
        }
    }
}
