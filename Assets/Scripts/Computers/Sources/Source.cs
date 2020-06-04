namespace Assets.Scripts.Computers.Sources
{
    public abstract class Source : ComputerComponent
    {
        public override int LoadUsage { get => 0; }
        public abstract int ProvidedLoad { get; }

        public override string ToString()
        {
            return $"{Name} {ProvidedLoad}W";
        }
    }
}