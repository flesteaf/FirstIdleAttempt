namespace Assets.Scripts.Commands.CommandSegments
{
    public class CommandSegment
    {
        public string Segment { get; }
        public bool HasQuotes { get; }
     
        public CommandSegment(string segment, bool hasQuotes = false)
        {
            Segment = segment;
            HasQuotes = hasQuotes;
        }
    }
}
