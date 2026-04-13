using System.Collections.Generic;
using HackYourWay.Models;

namespace HackYourWay.Core
{
    /// <summary>
    /// Calculates income earned while the game was closed.
    /// Uses <c>max(malware.InstalledAtUtcTicks, lastSaveUtcTicks)</c> per malware
    /// to avoid over-counting income for malware installed mid-session.
    /// No allocations beyond the returned list.
    /// </summary>
    public static class OfflineIncomeCalculator
    {
        /// <summary>
        /// Computes offline income for all active income-generating malware.
        /// </summary>
        /// <param name="lastSaveUtcTicks">Player.LastSaveUtcTicks from the loaded save.</param>
        /// <param name="malwareList">All active Malware instances across all infected devices.</param>
        /// <param name="nowUtcTicks">Current UTC ticks (pass DateTime.UtcNow.Ticks in production).</param>
        /// <returns>Accumulated income per currency since last session.</returns>
        public static List<CurrencyBalance> Calculate(
            long lastSaveUtcTicks,
            List<Malware> malwareList,
            long nowUtcTicks)
        {
            var results = new List<CurrencyBalance>();

            for (int i = 0; i < malwareList.Count; i++)
            {
                Malware m = malwareList[i];

                if (m.IncomeRate <= 0)
                    continue; // Bot and Ransomware generate no income.

                long baselineTicks = m.InstalledAtUtcTicks > lastSaveUtcTicks
                    ? m.InstalledAtUtcTicks
                    : lastSaveUtcTicks;

                double elapsedSeconds =
                    (nowUtcTicks - baselineTicks) / (double)System.TimeSpan.TicksPerSecond;

                if (elapsedSeconds <= 0)
                    continue;

                double income = elapsedSeconds * m.IncomeRate;
                AddToResults(results, m.Currency, income);
            }

            return results;
        }

        private static void AddToResults(List<CurrencyBalance> results, CurrencyType currency, double amount)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Currency == currency)
                {
                    results[i] = new CurrencyBalance(currency, results[i].Amount + amount);
                    return;
                }
            }
            results.Add(new CurrencyBalance(currency, amount));
        }
    }
}
