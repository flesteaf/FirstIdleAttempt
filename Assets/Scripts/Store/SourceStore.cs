using Assets.Scripts.Computers;

namespace Assets.Scripts.Store
{
    public class SourceStore : StoreComponent
    {
        private Source source;

        public Source Source
        {
            get => source;
            set
            {
                if (source == null)
                    source = value;
            }
        }
    }
}