using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services
{
    /// <summary>
    /// Drives passive income each tick for all income-generating malware.
    /// Registered with <see cref="HackYourWay.Core.TickManager"/> by GameManager.
    /// Uses a <c>for</c>-loop (no LINQ) to avoid per-tick GC allocations
    /// (Constitution Principle IV; Unity per-frame allocation guidance).
    /// </summary>
    public class IncomeService : ITickable
    {
        private readonly Player          _player;
        private readonly LocationService _locationService;

        public IncomeService(Player player, LocationService locationService)
        {
            _player          = player;
            _locationService = locationService;
        }

        /// <inheritdoc/>
        public void OnTick(double deltaSeconds)
        {
            Location loc = _locationService.GetCurrentLocation();

            for (int n = 0; n < loc.Networks.Count; n++)
            {
                var devices = loc.Networks[n].Devices;
                for (int d = 0; d < devices.Count; d++)
                {
                    Malware m = devices[d].ActiveMalware;
                    if (m == null || m.IncomeRate <= 0) continue;

                    // Only Miner and Spammer generate per-tick income.
                    if (m.Type != MalwareType.Miner && m.Type != MalwareType.Spammer)
                        continue;

                    _player.AddBalance(m.Currency, m.IncomeRate * deltaSeconds);
                }
            }
        }
    }
}
