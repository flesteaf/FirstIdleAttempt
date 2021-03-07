using System.Collections.Generic;

namespace Assets.Scripts.Softwares
{
    public class Software
    {
        private string name;
        private string description;
        private float price = -1;
        private List<Provides> provides;
        private long size = -1;

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrEmpty(name))
                    name = value;
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

        public float Price
        {
            get => price;
            set
            {
                if (price == -1)
                    price = value;
            }
        }

        public List<Provides> Provides
        {
            get => provides;
            set
            {
                provides = value;
            }
        }

        public long Size
        {
            get => size;
            set
            {
                if (size == -1)
                    size = value;
            }
        }
    }
}