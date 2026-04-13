using System.Collections.Generic;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services
{
    /// <summary>
    /// Manages contract availability, acceptance, and completion.
    /// Availability for malware-gated contracts is determined by tracking
    /// which malware types the player has active across all infected devices.
    /// </summary>
    public class ContractService
    {
        private readonly Player _player;

        // Transient registry of active malware types in the current session.
        private readonly HashSet<MalwareType> _activeMalwareTypes = new HashSet<MalwareType>();

        public ContractService(Player player)
        {
            _player = player;
        }

        /// <summary>
        /// Informs the service that the player has at least one active malware
        /// of the given type (call when malware is injected or on session restore).
        /// </summary>
        public void RegisterActiveMalware(MalwareType type)
        {
            _activeMalwareTypes.Add(type);
        }

        /// <summary>
        /// Returns true if the contract's prerequisites are satisfied.
        /// </summary>
        public bool IsAvailable(Contract contract)
        {
            if (!contract.HasMalwareRequirement) return true;
            return _activeMalwareTypes.Contains(contract.RequiredMalware);
        }

        /// <summary>
        /// Accepts a contract, adding it to <see cref="Player.ActiveContracts"/>.
        /// </summary>
        public CommandResult Accept(Contract contract)
        {
            if (_player.ActiveContracts.Contains(contract))
                return CommandResult.Fail($"Contract '{contract.Id}' is already active.");

            if (_player.CompletedContracts.Contains(contract.Id))
                return CommandResult.Fail($"Contract '{contract.Id}' is already completed.");

            _player.ActiveContracts.Add(contract);
            return CommandResult.Ok($"Contract accepted: {contract.Description}");
        }

        /// <summary>
        /// Checks if all objectives are complete and, if so, rewards the player.
        /// </summary>
        public CommandResult TryComplete(Contract contract)
        {
            // All objectives must be complete.
            for (int i = 0; i < contract.Objectives.Count; i++)
            {
                if (!contract.Objectives[i].IsComplete)
                    return CommandResult.Fail(
                        $"Objective '{contract.Objectives[i].Description}' is not yet complete.");
            }

            // Mark complete, remove from active, credit reward.
            contract.IsCompleted = true;
            _player.ActiveContracts.Remove(contract);
            _player.CompletedContracts.Add(contract.Id);
            _player.AddBalance(contract.RewardCurrency, contract.Reward);

            return CommandResult.Ok(
                $"Contract complete! Reward: {contract.Reward:F4} {contract.RewardCurrency} credited.");
        }

        /// <summary>Returns all contracts available to the player from the given list.</summary>
        public List<Contract> GetAvailable(IList<Contract> allContracts)
        {
            var result = new List<Contract>();
            for (int i = 0; i < allContracts.Count; i++)
            {
                var c = allContracts[i];
                if (!c.IsCompleted && !_player.CompletedContracts.Contains(c.Id) && IsAvailable(c))
                    result.Add(c);
            }
            return result;
        }
    }
}
