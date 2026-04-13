using UnityEngine;
using HackYourWay.Models;

namespace HackYourWay.Data
{
    [CreateAssetMenu(fileName = "Contract", menuName = "HackYourWay/Contract")]
    public class ContractSO : ScriptableObject
    {
        public string      Id;
        public ContractType Type;
        [TextArea] public string Description;
        public bool        HasMalwareRequirement;
        public MalwareType RequiredMalware;
        public double      Reward;
        public CurrencyType RewardCurrency;

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