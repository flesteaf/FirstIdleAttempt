using System.Text;

namespace HackYourWay.Models
{
    /// <summary>Result returned by <c>LocationService.ForgetNetwork</c> and <c>LocationService.ForgetDevice</c>.</summary>
    public readonly struct ForgetResult
    {
        /// <summary>True when the entity was found and removed.</summary>
        public readonly bool Success;

        /// <summary>User-facing confirmation or error message.</summary>
        public readonly string Message;

        /// <summary>Number of infected IPs whose income was stopped (0 for clean devices).</summary>
        public readonly int IpsRemoved;

        public ForgetResult(bool success, string message, int ipsRemoved = 0)
        {
            Success    = success;
            Message    = message;
            IpsRemoved = ipsRemoved;
        }

        /// <summary>Returns a failure result indicating the SSID matches networks at multiple locations.</summary>
        /// <param name="locationNames">Names of all locations where the SSID was found.</param>
        public static ForgetResult Ambiguous(string[] locationNames)
        {
            var sb = new StringBuilder("Network found at multiple locations:");
            for (int i = 0; i < locationNames.Length; i++)
                sb.Append(' ').Append(locationNames[i]);
            return new ForgetResult(false, sb.ToString(), 0);
        }
    }
}
