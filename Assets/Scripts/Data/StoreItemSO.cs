using UnityEngine;
using HackYourWay.Models;

namespace HackYourWay.Data
{
    /// <summary>Designer-editable definition of a store item.</summary>
    [CreateAssetMenu(fileName = "StoreItem", menuName = "HackYourWay/Store Item")]
    public class StoreItemSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public StoreItemCategory Category;
        public double Price;
        public CurrencyType PriceCurrency = CurrencyType.Bitcoin;

        [Header("Software — Tool Unlock")]
        public bool HasToolUnlock;
        public ToolType UnlocksToolType;

        [Header("Milestone — Currency Unlock")]
        public bool HasCurrencyUnlock;
        public CurrencyType UnlocksCurrency;

        [Header("PC Component — Income Multiplier")]
        [Min(1f)] public double IncomeMultiplier = 1.0;

        [Header("Hardware Upgrade")]
        public bool         HasHardwareUpgrade;
        public HardwareStat HardwareStatAffected;
        public int          HardwareTierGranted;

        /// <summary>Converts this SO into a runtime <see cref="StoreItem"/> model.</summary>
        public StoreItem ToModel()
        {
            return new StoreItem
            {
                Id                = Id,
                DisplayName       = DisplayName,
                Description       = Description,
                Category          = Category,
                Price             = Price,
                PriceCurrency     = PriceCurrency,
                HasToolUnlock     = HasToolUnlock,
                UnlocksToolType   = UnlocksToolType,
                HasCurrencyUnlock    = HasCurrencyUnlock,
                UnlocksCurrency     = UnlocksCurrency,
                IncomeMultiplier    = IncomeMultiplier,
                HasHardwareUpgrade  = HasHardwareUpgrade,
                HardwareStatAffected = HardwareStatAffected,
                HardwareTierGranted = HardwareTierGranted
            };
        }
    }
}
