using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
    public static class QueueExtensions
    {
        public static string ToText(this Queue<string> queue)
        {
            StringBuilder builder = new StringBuilder();
            List<string> listedQueue = queue.ToList();
            foreach (var item in listedQueue)
            {
                builder.AppendLine(item);
            }

            return builder.ToString();
        }
    }
}
