using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.Networks
{
    internal abstract class Network : ComputerComponent
    {
        internal abstract float Speed { get; }
        internal abstract Sizes SpeedType { get; }

        public override string ToString()
        {
            return $"{Name} {Speed}{SpeedType}";
        }
    }
}