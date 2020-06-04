namespace Assets.Scripts.Computers.Motherboards
{
    public abstract class Motherboard : ComputerComponent
    {
        public abstract int AllowedCpus { get; }
        public abstract int AllowedRams { get; }
        public abstract int AllowedHards { get; }
        public abstract int AllowedGpus { get; }

        //public abstract int AllowedWirelesses { get; }
        public abstract int AllowedNetworks { get; }

        public override string ToString()
        {
            return $"{Name} accepting {AllowedCpus} CPUs, {AllowedRams} RAMs, {AllowedHards} Storages, {AllowedGpus} GPUs, {AllowedNetworks} networks";
        }
    }
}