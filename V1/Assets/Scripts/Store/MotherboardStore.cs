using Assets.Scripts.Computers;

namespace Assets.Scripts.Store
{
    public class MotherboardStore : StoreComponent
    {
        private Motherboard motherboard;

        public Motherboard Motherboard
        {
            get => motherboard;
            set
            {
                if (motherboard == null)
                    motherboard = value;
            }
        }

        public override ComputerComponent SoldComponent => motherboard;
    }
}