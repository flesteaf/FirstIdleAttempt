using Assets.Scripts.Commands;

namespace Assets.Scripts.Software
{
    public class Software
    {
        public object Name { get; internal set; }
        public object Description { get; internal set; }
        public bool WasBought { get; internal set; }
        public float Price { get; private set; }
        public CommandOptions Provides { get; private set; }
    }
}
