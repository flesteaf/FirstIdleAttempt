using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services
{
    /// <summary>
    /// Drives passive income each tick for all income-generating malware across ALL known locations.
    /// Registered with <see cref="HackYourWay.Core.TickManager"/> by GameManager.
    /// Uses a <c>for</c>-loop over the pre-allocated snapshot (no LINQ, no per-tick allocation —
    /// Constitution Principle IV; Unity per-frame allocation guidance).
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
            var locations = _locationService.GetAllKnownLocations();

            for (int l = 0; l < locations.Count; l++)
            {
                var networks = locations[l].Networks;
                for (int n = 0; n < networks.Count; n++)
                {
                    var devices = networks[n].Devices;
                    for (int d = 0; d < devices.Count; d++)
                    {
                        Malware m = devices[d].ActiveMalware;
                        if (m == null || m.IncomeRate <= 0) continue;

                        if (m.Type != MalwareType.Miner && m.Type != MalwareType.Spammer)
                            continue;

                        _player.AddBalance(m.Currency, m.IncomeRate * deltaSeconds);
                    }
                }
            }
        }
    }
}
