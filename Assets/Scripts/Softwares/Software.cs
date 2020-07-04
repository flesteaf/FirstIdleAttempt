using Assets.Scripts.Commands;

namespace Assets.Scripts.Softwares
{
    public class Software
    {
        private string name;
        private string description;
        private float price = -1;
        private CommandOptions provides;
        private CommandNames commandName;

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

        public CommandOptions Provides
        {
            get => provides;
            set
            {
                if (provides == CommandOptions.Invalid)
                    provides = value;
            }
        }

        public CommandNames CommandName
        {
            get => commandName;
            set
            {
                if (commandName == CommandNames.invalid)
                    commandName = value;
            }
        }
    }
}