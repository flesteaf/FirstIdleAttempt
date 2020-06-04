using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.Networks
{
    public abstract class Network : ComputerComponent
    {
        public abstract float Speed { get; }
        public abstract Sizes SizeType { get; }

        public override string ToString()
        {
            return $"{Name} {Speed}{SizeType}";
        }
    }
}