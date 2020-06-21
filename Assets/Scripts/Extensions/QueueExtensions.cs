using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
    public static class QueueExtensions
    {
        public static string ToTextConsole(this Queue<string> queue, int acceptedNoOfLines)
        {
            StringBuilder builder = new StringBuilder();
            List<string> listedQueue = queue.ToList();
            for (int i = 0; i < acceptedNoOfLines && i < listedQueue.Count; i++)
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