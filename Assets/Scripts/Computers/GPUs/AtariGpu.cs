using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.GPUs
{
    public class AtariGpu : Gpu
    {
        public override string Name => "Atari first GPU";

        public override int LoadUsage => 15;

        public override RamType RamType => RamType.DDR2;

        public override float Size => 1;

        public override Sizes SizeType => Sizes.MB;
    }
}