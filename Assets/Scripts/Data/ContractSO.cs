using Godot;
using HackYourWay.Models;

namespace HackYourWay.Data
{
    [GlobalClass]
    public partial class ContractSO : Resource
    {
        [Export] public string Id { get; set; }
        [Export] public ContractType Type { get; set; }
        [Export(PropertyHint.MultilineText)] public string Description { get; set; }
        [Export] public bool HasMalwareRequirement { get; set; }
        [Export] public MalwareType RequiredMalware { get; set; }
        [Export] public double Reward { get; set; }
        [Export] public CurrencyType RewardCurrency { get; set; }

        public Contract ToModel()
        {
            return new Contract
            {
                Id                    = Id,
                Type                  = Type,
                Description           = Description,
                HasMalwareRequirement = HasMalwareRequirement,
                RequiredMalware       = RequiredMalware,
                Reward                = Reward,
                RewardCurrency        = RewardCurrency
            };
        }
    }
}
