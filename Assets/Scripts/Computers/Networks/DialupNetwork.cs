using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.Networks
{
    internal class DialupNetwork : Network
    {
        internal override float Speed => 120;

        internal override Sizes SizeType => Sizes.KB;

        internal override string Name => "Dial-up network";

        internal override int LoadUsage => 7;
    }
}