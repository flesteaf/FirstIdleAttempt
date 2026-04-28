namespace HackYourWay.Models
{
    /// <summary>Runtime representation of a purchasable store item.</summary>
    [System.Serializable]
    public class StoreItem
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public StoreItemCategory Category;
        public double Price;
        public CurrencyType PriceCurrency;

        /// <summary>Tool unlocked on purchase (Software items).</summary>
        public ToolType UnlocksToolType;
        public bool HasToolUnlock;

        /// <summary>Currency unlocked on purchase (milestone items).</summary>
        public CurrencyType UnlocksCurrency;
        public bool HasCurrencyUnlock;

        /// <summary>Multiplier applied to all income rates (PCComponent items).</summary>
        public double IncomeMultiplier;

        /// <summary>True when this item upgrades a player hardware tier.</summary>
        public bool HasHardwareUpgrade;

        /// <summary>Which hardware stat this item upgrades (CPU, Bandwidth, GPU).</summary>
        public HardwareStat HardwareStatAffected;

        /// <summary>The tier the player reaches on purchase (replaces current tier).</summary>
        public int HardwareTierGranted;
    }
}
