using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
    public static class QueueExtensions
    {
        public static string ToTextConsole(this Queue<string> queue)
        {
            StringBuilder builder = new StringBuilder();
            List<string> listedQueue = queue.ToList();
            foreach (var item in listedQueue)
            {
                if (string.IsNullOrEmpty(item))
                    continue;

                builder.AppendLine(item);
            }

            return builder.ToString();
        }
    }
}
