namespace Assets.Scripts.Computers.Motherboards
{
    internal abstract class Motherboard : ComputerComponent
    {
        internal abstract int AllowedCpus { get; }
        internal abstract int AllowedRams { get; }
        internal abstract int AllowedHards { get; }
        internal abstract int AllowedGpus { get; }

        //internal abstract int AllowedWirelesses { get; }
        internal abstract int AllowedNetworks { get; }

        public override string ToString()
        {
            return $"{Name} accepting {AllowedCpus} CPUs, {AllowedRams} RAMs, {AllowedHards} Storages, {AllowedGpus} GPUs, {AllowedNetworks} networks";
        }
    }
}