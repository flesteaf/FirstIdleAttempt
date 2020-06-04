using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.CPUs
{
    public class Cpu : ComputerComponent
    {
        public int Cores { get; }
        public float Speed { get; }
        public SpeedType SpeedType { get; }

        public override string Name { get; set; }

        public override int LoadUsage { get; set; }

        public float TotalSpeed()
        {
            return Cores * Speed * (float)SpeedType;
        }

        public override string ToString()
        {
            return $"{Name} with {Cores} cores at {Speed} {SpeedType}";
        }
    }
}