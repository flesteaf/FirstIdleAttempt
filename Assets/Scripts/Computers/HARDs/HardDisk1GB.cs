using Assets.Scripts.Computers.ComponentTypes;

namespace Assets.Scripts.Computers.HARDs
{
    public class HardDisk1GB : Hard
    {
        public override HardType HardType => HardType.HardDisk;

        public override float Size => 1;

        //TODO: think of a better name
        public override string Name => "First hard";

        public override int LoadUsage => 5;

        public override Sizes SizeType => Sizes.GB;
    }
}