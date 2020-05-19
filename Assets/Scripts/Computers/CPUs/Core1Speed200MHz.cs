using Assets.Scripts.Computers.ComponentTypes;
using Assets.Scripts.Computers.CPUs;

namespace Assets.Scripts.Computers
{
    internal class Core1Speed200MHz : Cpu
    {
        protected override SpeedType SpeedType => SpeedType.MHz;

        internal override int Cores => 1;

        internal override float Speed => 200;

        //TODO: think of a better name
        internal override string Name => "The1 200";

        internal override int LoadUsage => 15;
    }
}