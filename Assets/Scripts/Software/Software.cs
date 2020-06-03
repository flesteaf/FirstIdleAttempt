using Assets.Scripts.Commands;

namespace Assets.Scripts.Software
{
    internal class Software
    {
        public object Name { get; internal set; }
        public object Description { get; internal set; }
        public bool WasBought { get; internal set; }
        internal float Price { get; private set; }
        internal CommandOptions Provides { get; private set; }
    }
}
