using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers
{
    public class Cpu : ComputerComponent
    {
        private int cores = -1;
        private float speed = -1;
        private SpeedType speedType = SpeedType.None;

        public int Cores
        {
            get => cores;
            set
            {
                if (cores == -1)
                    cores = value;
            }
        }

        public float Speed
        {
            get => speed;
            set
            {
                if (speed == -1)
                    speed = value;
            }
        }

        public SpeedType SpeedType
        {
            get => speedType;
            set
            {
                if (speedType == SpeedType.None)
                    speedType = value;
            }
        }

        public int TotalSpeed()
        {
            return (int)(Cores * Speed * (int)SpeedType);
        }

        public override string ToString()
        {
            return $"{Name} with {Cores} cores at {Speed} {SpeedType}";
        }
    }
}