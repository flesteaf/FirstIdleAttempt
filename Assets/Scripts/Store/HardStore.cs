using Assets.Scripts.Computers;

namespace Assets.Scripts.Store
{
    public class HardStore : StoreComponent
    {
        private Hard hard;

        public Hard Hard
        {
            get => hard;
            set
            {
                if (hard == null)
                    hard = value;
            }
        }
    }
}