using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string ToTextConsole(this IEnumerable<string> queue, int acceptedNoOfLines)
        {
            StringBuilder builder = new StringBuilder();
            List<string> listedQueue = queue.ToList();

            int startIndex = 0;
            if (listedQueue.Count >= acceptedNoOfLines)
            {
                startIndex = listedQueue.Count - acceptedNoOfLines;
            }

            for (int i = startIndex; i < listedQueue.Count; i++)
            {
                string item = listedQueue[i];
                if (string.IsNullOrEmpty(item))
                    continue;

                builder.AppendLine(item);
            }

            return builder.ToString();
        }
    }
}