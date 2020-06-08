using Assets.Scripts.Computers;

namespace Assets.Scripts.Store
{
    public class RamStore : StoreComponent
    {
        private Ram ram;

        public Ram RAM
        {
            get => ram;
            set
            {
                if (ram == null)
                    ram = value;
            }
        }
    }
}