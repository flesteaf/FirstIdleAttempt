using Assets.Scripts.Computers.Motherboards;

namespace Assets.Scripts.Computers
{
    public class SmallMotherboard : Motherboard
    {
        public override int AllowedCpus => 1;

        public override int AllowedRams => 2;

        public override int AllowedHards => 1;

        public override int AllowedGpus => 1;

        //public override int AllowedWirelesses => 0;

        public override int AllowedNetworks => 1;

        public override string Name => "Small motherboard";

        public override int LoadUsage => 3;
    }
}