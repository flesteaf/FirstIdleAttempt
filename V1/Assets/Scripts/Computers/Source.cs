namespace Assets.Scripts.Computers
{
    public class Source : ComputerComponent
    {
        private int providedLoad = -1;

        public int ProvidedLoad
        {
            get => providedLoad;
            set
            {
                if (providedLoad == -1)
                    providedLoad = value;
            }
        }

        public Source()
        {
            LoadUsage = 0;
        }

        public override string ToString()
        {
            return $"{Name} {ProvidedLoad}W";
        }
    }
}