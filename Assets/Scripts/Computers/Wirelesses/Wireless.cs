using Assets.Scripts.Computers.ComponentTypes;
using Assets.Scripts.Computers.Networks;

namespace Assets.Scripts.Computers.Wirelesses
{
    public abstract class Wireless : Network
    {
        public abstract WirelessType WirelessType { get; }

        public override string ToString()
        {
            return $"Wireless {base.ToString()}";
        }
    }
}