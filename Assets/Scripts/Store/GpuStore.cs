using Assets.Scripts.Computers;

namespace Assets.Scripts.Store
{
    public class GpuStore : StoreComponent
    {
        private Gpu gpu;

        public Gpu GPU
        {
            get => gpu;
            set
            {
                if (gpu == null)
                    gpu = value;
            }
        }
    }
}