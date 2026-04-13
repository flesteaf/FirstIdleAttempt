using System.Collections.Generic;

namespace HackYourWay.Models
{
    /// <summary>A player contract with objectives and a reward on completion.</summary>
    [System.Serializable]
    public class Contract
    {
        public string Id;
        public ContractType Type;
        public string Description;

        /// <summary>Malware that must be active on any device before this contract is available.</summary>
        public MalwareType RequiredMalware;

        /// <summary>Whether RequiredMalware is actually needed (some contracts are open).</summary>
        public bool HasMalwareRequirement;

        public List<Objective> Objectives = new List<Objective>();

        public double Reward;
        public CurrencyType RewardCurrency;
        public bool IsCompleted;
    }

    /// <summary>A single step within a <see cref="Contract"/>.</summary>
    [System.Serializable]
    public class Objective
    {
        public string Description;
        public bool IsComplete;
    }
}
