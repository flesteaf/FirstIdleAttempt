namespace Assets.Scripts.Computers.Sources
{
    internal abstract class Source
    {
        internal abstract string Name { get; }
        internal abstract int ProvidedLoad { get; }

        public override string ToString()
        {
            return $"{Name} {ProvidedLoad}W";
        }
    }
}
