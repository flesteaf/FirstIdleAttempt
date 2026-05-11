using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services
{
    /// <summary>
    /// Validates and processes player purchases from the store catalog.
    /// Applies item effects immediately on successful purchase:
    /// tool unlock, currency unlock, or income multiplier.
    /// </summary>
    public class StoreService
    {
        private readonly Player _player;

        /// <summary>
        /// Combined income multiplier from all purchased PC component upgrades.
        /// Starts at 1.0 (no multiplier). Applied by IncomeService on each tick.
        /// </summary>
        public double CurrentIncomeMultiplier { get; private set; } = 1.0;

        public StoreService(Player player)
        {
            _player = player;
        }

        /// <summary>
        /// Attempts to purchase <paramref name="item"/> for the current player.
        /// Returns a failed result without modifying state if any precondition fails.
        /// </summary>
        public CommandResult Purchase(StoreItem item)
        {
            // Already owned?
            if (_player.PurchasedItems.Contains(item.Id))
                return CommandResult.Fail($"'{item.DisplayName}' is already purchased.");

            // Sufficient balance?
            double balance = _player.GetBalance(item.PriceCurrency);
            if (balance < item.Price)
                return CommandResult.Fail(
                    $"Insufficient funds. '{item.DisplayName}' costs {item.Price:F4} {item.PriceCurrency}. " +
                    $"You have {balance:F4}.");

            // Deduct and record.
            _player.AddBalance(item.PriceCurrency, -item.Price);
            _player.PurchasedItems.Add(item.Id);

            // Apply effect.
            ApplyEffect(item);

            return CommandResult.Ok($"Purchased '{item.DisplayName}'.");
        }

        // ── Effect application ────────────────────────────────────────────────

        private void ApplyEffect(StoreItem item)
        {
            if (item.HasToolUnlock)
            {
                if (!_player.HasTool(item.UnlocksToolType))
                    _player.UnlockedTools.Add(item.UnlocksToolType);
            }

            if (item.HasCurrencyUnlock)
            {
                if (!_player.HasCurrency(item.UnlocksCurrency))
                    _player.UnlockedCurrencies.Add(item.UnlocksCurrency);
            }

            if (item.IncomeMultiplier > 1.0)
                CurrentIncomeMultiplier *= item.IncomeMultiplier;

            if (item.HasHardwareUpgrade)
                ApplyHardwareTier(item.HardwareStatAffected, item.HardwareTierGranted);
        }

        private void ApplyHardwareTier(Models.HardwareStat stat, int tier)
        {
            switch (stat)
            {
                case Models.HardwareStat.CPU:
                    if (tier > _player.CpuTier) _player.CpuTier = tier;
                    break;
                case Models.HardwareStat.Bandwidth:
                    if (tier > _player.BandwidthTier) _player.BandwidthTier = tier;
                    break;
                case Models.HardwareStat.GPU:
                    if (tier > _player.GpuTier) _player.GpuTier = tier;
                    break;
            }
        }
    }
}
