using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.Networks
{
    public class DialupNetwork : Network
    {
        public override float Speed => 120;

        public override Sizes SizeType => Sizes.KB;

        public override string Name => "Dial-up network";

        public override int LoadUsage => 7;
    }
}