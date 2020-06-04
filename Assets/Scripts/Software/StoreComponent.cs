using Assets.Scripts.Computers;

namespace Assets.Scripts.Software
{
    internal class StoreComponent
    {
        internal ComputerComponent Component { get; private set; }
        internal float Price { get; private set; }
        public string Description { get; internal set; }
        public bool WasBought { get; internal set; }

        public StoreComponent(ComputerComponent component, 
                              float price,
                              string description)
        {
            Component = component;
            Price = price;
            Description = description;
        }
    }
}
