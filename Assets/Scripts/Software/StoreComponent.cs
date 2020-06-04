using Assets.Scripts.Computers;

namespace Assets.Scripts.Software
{
    public class StoreComponent
    {
        public ComputerComponent Component { get; private set; }
        public float Price { get; private set; }
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
