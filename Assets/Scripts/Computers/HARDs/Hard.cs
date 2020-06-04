using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.HARDs
{
    public abstract class Hard : ComputerComponent
    {
        public abstract HardType HardType { get; }
        public abstract float Size { get; }
        public abstract Sizes SizeType { get; }

        public override string ToString()
        {
            return $"{Name} {Size}{SizeType} {HardType}";
        }
    }
}