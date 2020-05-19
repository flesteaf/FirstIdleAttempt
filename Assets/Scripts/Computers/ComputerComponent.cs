namespace Assets.Scripts.Computers
{
    internal abstract class ComputerComponent
    {
        internal abstract string Name { get; }
        internal abstract int LoadUsage { get; }
    }
}
