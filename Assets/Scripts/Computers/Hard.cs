using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers
{
    public class Hard : ComputerComponent
    {
        private HardType hardType = HardType.None;
        private float size = -1;
        private Sizes sizeType = Sizes.None;

        public HardType HardType
        {
            get => hardType;
            set
            {
                if (hardType == HardType.None)
                    hardType = value;
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
            return $"{Name} {Size}{SizeType} {HardType}";
        }
    }
}