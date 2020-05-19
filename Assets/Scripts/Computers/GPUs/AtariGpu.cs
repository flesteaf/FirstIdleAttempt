using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.GPUs
{
    internal class AtariGpu : Gpu
    {
        internal override string Name => "Atari first GPU";

        internal override int LoadUsage => 15;

        internal override RamType RamType => RamType.DDR2;

        internal override float Size => 1;

        internal override Sizes SizeType => Sizes.MB;
    }
}