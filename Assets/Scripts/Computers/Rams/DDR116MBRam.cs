using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.Rams
{
    public class DDR116MBRam : Ram
    {
        public override RamType RamType { get => RamType.DDR1; }
        public override float Size { get => 16; }

        public override Sizes SizeType => Sizes.MB;

        //TODO: think of a better name
        public override string Name => "First Ram";

        public override int LoadUsage => 5;
    }
}