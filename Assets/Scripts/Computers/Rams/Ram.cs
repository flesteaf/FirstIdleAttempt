using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.Rams
{
    internal abstract class Ram : ComputerComponent
    {
        public abstract RamType Type { get;  }
        public abstract float Size { get; }
        public abstract Sizes SizeType { get; }

        public override string ToString()
        {
            return $"{Name} {Size}{SizeType} {Type}";
        }
    }
}