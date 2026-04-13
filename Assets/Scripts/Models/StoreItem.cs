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
    }
}
