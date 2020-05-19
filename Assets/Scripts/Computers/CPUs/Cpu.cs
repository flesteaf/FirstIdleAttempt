using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.CPUs
{
    internal abstract class Cpu : ComputerComponent
    {
        internal abstract int Cores { get;  }
        internal abstract float Speed { get;  }
        protected abstract SpeedType SpeedType { get; }

        internal virtual float TotalSpeed()
        {
            return Cores * Speed * (float)SpeedType;
        }

        public override string ToString()
        {
            return $"{Name} with {Cores} cores at {Speed} {SpeedType}";
        }
    }
}