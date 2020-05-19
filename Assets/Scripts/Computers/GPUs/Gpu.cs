using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.GPUs
{
    internal abstract class Gpu : ComputerComponent
    {
        internal abstract RamType RamType { get; }
        internal abstract float Size { get; }
        internal abstract Sizes SizeType { get; }

        public override string ToString()
        {
            return $"{Name} {Size}{SizeType} {RamType}";
        }
    }
}