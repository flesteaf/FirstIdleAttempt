using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers
{
    public class NetworkBoard : ComputerComponent
    {
        private float speed = -1;
        private Sizes sizeType;

        public float Speed
        {
            get => speed;
            set
            {
                if (speed == -1)
                    speed = value;
            }
        }

        public Sizes SizeType
        {
            get => sizeType;
            set
            {
                if (sizeType == Sizes.None)
                    sizeType = value;
            }
        }

        public override string ToString()
        {
            return $"{Name} {Speed}{SizeType}";
        }
    }
}