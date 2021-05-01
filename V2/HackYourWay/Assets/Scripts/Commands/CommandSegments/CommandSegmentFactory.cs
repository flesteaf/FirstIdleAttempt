using System.Collections.Generic;

namespace Assets.Scripts.Commands.CommandSegments
{
    public static class CommandSegmentFactory
    {
        public static IEnumerable<CommandSegment> CreateSegments(string command)
        {
            string[] segments = command.Split(new[] { '"', '\'' });

            for (int i = 0; i < segments.Length; i++)
            {
                if (!string.IsNullOrEmpty(segments[i]))
                {
                    yield return new CommandSegment(segments[0], (i % 2 != 0));
                }
            }
        }
    }
}
