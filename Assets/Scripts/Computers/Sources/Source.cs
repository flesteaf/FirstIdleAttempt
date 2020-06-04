namespace Assets.Scripts.Computers.Sources
{
    internal abstract class Source : ComputerComponent
    {
        internal override int LoadUsage { get => 0; }
        internal abstract int ProvidedLoad { get; }

        public override string ToString()
        {
            return $"{Name} {ProvidedLoad}W";
        }
    }
}