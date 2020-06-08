using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers
{
    public class Ram : ComputerComponent
    {
        private RamType ramType;
        private float size = -1;
        private Sizes sizeType;

        public RamType RamType
        {
            get => ramType;
            set
            {
                if (ramType == RamType.None)
                    ramType = value;
            }
        }

        public float Size
        {
            get => size;
            set
            {
                if (size == -1)
                    size = value;
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
            return $"{Name} {Size}{SizeType} {RamType}";
        }
    }
}