namespace HackYourWay.Models
{
    /// <summary>Wi-Fi security protocol of a <see cref="Network"/>.</summary>
    public enum SecurityLevel
    {
        /// <summary>No security — no crack tool required.</summary>
        None = 0,
        WEP  = 1,
        WPA  = 2,
        WPA2 = 3
    }
}
