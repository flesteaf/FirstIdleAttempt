using Assets.Scripts.Computers.ComponentTypes;
using Assets.Scripts.Computers.Networks;

namespace Assets.Scripts.Computers.Wirelesses
{
    internal abstract class Wireless : Network
    {
        internal abstract WirelessType Type { get; }

        public override string ToString()
        {
            return $"Wireless {base.ToString()}";
        }
    }
}