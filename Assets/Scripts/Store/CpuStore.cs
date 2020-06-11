using Assets.Scripts.Computers;

namespace Assets.Scripts.Store
{
    public class CpuStore : StoreComponent
    {
        private Cpu cpu;

        public Cpu CPU
        {
            get => cpu;
            set
            {
                if (cpu == null)
                    cpu = value;
            }
        }

        public override ComputerComponent SoldComponent => cpu;
    }
}