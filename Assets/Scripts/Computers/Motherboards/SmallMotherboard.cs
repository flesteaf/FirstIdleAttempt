using Assets.Scripts.Computers.Motherboards;

namespace Assets.Scripts.Computers
{
    internal class SmallMotherboard : Motherboard
    {
        internal override int AllowedCpus => 1;

        internal override int AllowedRams => 2;

        internal override int AllowedHards => 1;

        internal override int AllowedGpus => 1;

        //internal override int AllowedWirelesses => 0;

        internal override int AllowedNetworks => 1;

        internal override string Name => "Small motherboard";

        internal override int LoadUsage => 3;
    }
}