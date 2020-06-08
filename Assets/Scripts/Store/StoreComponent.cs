namespace Assets.Scripts.Store
{
    public class StoreComponent
    {
        private float price = -1;
        private string description;

        public float Price
        {
            get => price;
            set
            {
                if (price == -1)
                    price = value;
            }
        }

        public string Description
        {
            get => description;
            set
            {
                if (string.IsNullOrEmpty(description))
                    description = value;
            }
        }

        public bool WasBought { get; set; }
    }
}