using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.HARDs
{
    internal class HardDisk1GB : Hard
    {
        internal override HardType Type => HardType.HardDisk;

        internal override float Size => 1;

        //TODO: think of a better name
        internal override string Name => "First hard";

        internal override int LoadUsage => 5;

        internal override Sizes SizeType => Sizes.GB;
    }
}