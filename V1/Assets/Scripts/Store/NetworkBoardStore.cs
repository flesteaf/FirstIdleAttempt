using Assets.Scripts.Computers;

namespace Assets.Scripts.Store
{
    public class NetworkBoardStore : StoreComponent
    {
        private NetworkBoard network;

        public NetworkBoard Network
        {
            get => network;
            set
            {
                if (network == null)
                    network = value;
            }
        }

        public override ComputerComponent SoldComponent => network;
    }
}