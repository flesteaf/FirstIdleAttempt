namespace Assets.Scripts.Computers
{
    public abstract class ComputerComponent
    {
        public abstract string Name { get; set; }
        public abstract int LoadUsage { get; set; }
    }
}