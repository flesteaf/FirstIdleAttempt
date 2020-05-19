using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.Rams
{
    internal class DDR116MBRam : Ram
    {
        public override RamType Type { get => RamType.DDR1; }
        public override float Size { get => 16; }

        public override Sizes SizeType => Sizes.MB;

        //TODO: think of a better name
        internal override string Name => "First Ram";

        internal override int LoadUsage => 5;
    }
}