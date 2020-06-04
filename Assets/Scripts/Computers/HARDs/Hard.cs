using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.HARDs
{
    internal abstract class Hard : ComputerComponent
    {
        internal abstract HardType HardType { get; }
        internal abstract float Size { get; }
        internal abstract Sizes SizeType { get; }

        public override string ToString()
        {
            return $"{Name} {Size}{SizeType} {HardType}";
        }
    }
}