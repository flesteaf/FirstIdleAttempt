using Godot;
using HackYourWay.Models;

namespace HackYourWay.Data
{
    /// <summary>Designer-editable definition of a store item.</summary>
    [GlobalClass]
    public partial class StoreItemSO : Resource
    {
        [Export] public string Id { get; set; }
        [Export] public string DisplayName { get; set; }
        [Export(PropertyHint.MultilineText)] public string Description { get; set; }
        [Export] public StoreItemCategory Category { get; set; }
        [Export] public double Price { get; set; }
        [Export] public CurrencyType PriceCurrency { get; set; } = CurrencyType.Bitcoin;

        [ExportGroup("Software — Tool Unlock")]
        [Export] public bool HasToolUnlock { get; set; }
        [Export] public ToolType UnlocksToolType { get; set; }

        [ExportGroup("Milestone — Currency Unlock")]
        [Export] public bool HasCurrencyUnlock { get; set; }
        [Export] public CurrencyType UnlocksCurrency { get; set; }

        [ExportGroup("PC Component — Income Multiplier")]
        [Export] public double IncomeMultiplier { get; set; } = 1.0;

        [ExportGroup("Hardware Upgrade")]
        [Export] public bool HasHardwareUpgrade { get; set; }
        [Export] public HardwareStat HardwareStatAffected { get; set; }
        [Export] public int HardwareTierGranted { get; set; }

        /// <summary>Converts this resource into a runtime <see cref="StoreItem"/> model.</summary>
        public StoreItem ToModel()
        {
            return new StoreItem
            {
                Id                   = Id,
                DisplayName          = DisplayName,
                Description          = Description,
                Category             = Category,
                Price                = Price,
                PriceCurrency        = PriceCurrency,
                HasToolUnlock        = HasToolUnlock,
                UnlocksToolType      = UnlocksToolType,
                HasCurrencyUnlock    = HasCurrencyUnlock,
                UnlocksCurrency      = UnlocksCurrency,
                IncomeMultiplier     = IncomeMultiplier,
                HasHardwareUpgrade   = HasHardwareUpgrade,
                HardwareStatAffected = HardwareStatAffected,
                HardwareTierGranted  = HardwareTierGranted
            };
        }
    }
}
