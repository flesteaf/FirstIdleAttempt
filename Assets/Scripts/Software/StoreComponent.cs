using Assets.Scripts.Computers;

namespace Assets.Scripts.Software
{
    internal class StoreComponent
    {
        internal ComputerComponent Component { get; private set; }
        internal float Price { get; private set; }
        public object Description { get; internal set; }
        public bool WasBought { get; internal set; }

        public StoreComponent(ComputerComponent component, float price)
        {
            Component = component;
            Price = price;
        }
    }
}
