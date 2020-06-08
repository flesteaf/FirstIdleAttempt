namespace Assets.Scripts.Computers
{
    public class Motherboard : ComputerComponent
    {
        private int allowedCpus = -1;
        private int allowedRams = -1;
        private int allowedHards = -1;
        private int allowedGpus = -1;
        private int allowedNetworks = -1;

        public int AllowedCpus
        {
            get => allowedCpus;
            set
            {
                if (allowedCpus == -1)
                    allowedCpus = value;
            }
        }

        public int AllowedRams
        {
            get => allowedRams;
            set
            {
                if (allowedRams == -1)
                    allowedRams = value;
            }
        }

        public int AllowedHards
        {
            get => allowedHards;
            set
            {
                if (allowedHards == -1)
                    allowedHards = value;
            }
        }

        public int AllowedGpus
        {
            get => allowedGpus;
            set
            {
                if (allowedGpus == -1)
                    allowedGpus = value;
            }
        }

        public int AllowedNetworks
        {
            get => allowedNetworks;
            set
            {
                if (allowedNetworks == -1)
                    allowedNetworks = value;
            }
        }

        public override string ToString()
        {
            return $"{Name} accepting {AllowedCpus} CPUs, {AllowedRams} RAMs, {AllowedHards} Storages, {AllowedGpus} GPUs, {AllowedNetworks} networks";
        }
    }
}