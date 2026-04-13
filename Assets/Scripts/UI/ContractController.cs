using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HackYourWay.Core;
using HackYourWay.Models;
using HackYourWay.Services;

namespace HackYourWay.UI
{
    /// <summary>
    /// Displays available and active contracts and handles Accept button events.
    /// Delegates all contract logic to <see cref="ContractService"/>.
    /// Feedback messages are sent to the <see cref="TerminalOutputView"/>.
    /// </summary>
    public class ContractController : MonoBehaviour
    {
        [SerializeField] private Transform          _contractListParent;
        [SerializeField] private GameObject         _contractRowPrefab;
        [SerializeField] private TerminalOutputView _terminalOutput;

        private ContractService      _contractService;
        private List<Contract>       _allContracts = new List<Contract>();

        private void Start()
        {
            if (GameManager.Instance != null)
                _contractService = new ContractService(GameManager.Instance.Player);

            LoadContracts();
            Refresh();
        }

        // ── Contract loading ──────────────────────────────────────────────────

        /// <summary>
        /// Loads contract definitions from Resources/Contracts/ as ContractSO assets.
        /// For now contracts are authored inline; extend to ContractSO if needed.
        /// </summary>
        private void LoadContracts()
        {
            var sos = Resources.LoadAll<HackYourWay.Data.ContractSO>("Contracts");
            for (int i = 0; i < sos.Length; i++)
                _allContracts.Add(sos[i].ToModel());
        }

        /// <summary>Registers a list of contracts for this controller to display.</summary>
        public void RegisterContracts(List<Contract> contracts)
        {
            _allContracts = contracts;
            Refresh();
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        private void Refresh()
        {
            if (_contractListParent == null || _contractRowPrefab == null || _contractService == null)
                return;

            // Clear existing rows.
            for (int i = _contractListParent.childCount - 1; i >= 0; i--)
                Destroy(_contractListParent.GetChild(i).gameObject);

            var available = _contractService.GetAvailable(_allContracts);

            for (int i = 0; i < available.Count; i++)
            {
                Contract contract = available[i];
                GameObject row    = Instantiate(_contractRowPrefab, _contractListParent);

                var labels = row.GetComponentsInChildren<TextMeshProUGUI>();
                if (labels.Length >= 2)
                {
                    labels[0].text = contract.Description;
                    labels[1].text = $"Reward: {contract.Reward:F4} {contract.RewardCurrency}";
                }

                var btn = row.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    Contract captured = contract;
                    btn.onClick.AddListener(() => OnAccept(captured));
                }
            }

            // Also display active contracts.
            if (GameManager.Instance != null)
            {
                var active = GameManager.Instance.Player.ActiveContracts;
                for (int i = 0; i < active.Count; i++)
                {
                    var row = Instantiate(_contractRowPrefab, _contractListParent);
                    var labels = row.GetComponentsInChildren<TextMeshProUGUI>();
                    if (labels.Length >= 1)
                        labels[0].text = $"[ACTIVE] {active[i].Description}";
                }
            }
        }

        // ── Accept handler ────────────────────────────────────────────────────

        private void OnAccept(Contract contract)
        {
            if (_contractService == null) return;

            var result = _contractService.Accept(contract);
            _terminalOutput?.AppendLine(result.Message);
            Refresh();
        }
    }
}
