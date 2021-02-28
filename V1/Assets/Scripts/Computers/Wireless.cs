using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers
{
    public abstract class Wireless : NetworkBoard
    {
        public abstract WirelessType WirelessType { get; }

        public override string ToString()
        {
            return $"Wireless {base.ToString()}";
        }
    }
}