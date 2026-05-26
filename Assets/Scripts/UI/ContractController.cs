using System.Collections.Generic;
using Godot;
using HackYourWay.Core;
using HackYourWay.Models;
using HackYourWay.Services;

namespace HackYourWay.UI
{
    /// <summary>
    /// Displays available and active contracts and handles Accept button events.
    /// Delegates all contract logic to <see cref="ContractService"/>.
    /// Feedback messages are sent to the <see cref="TerminalOutputView"/>.
    ///
    /// Expected row scene node layout:
    ///   ContractRow.tscn
    ///   ├── DescriptionLabel (Label)
    ///   ├── RewardLabel      (Label)
    ///   └── AcceptButton     (Button)
    /// </summary>
    public partial class ContractController : Control
    {
        [Export] private Node               _contractListParent;
        [Export] private PackedScene        _contractRowPrefab;
        [Export] private TerminalOutputView _terminalOutput;

        private ContractService      _contractService;
        private List<Contract>       _allContracts = new List<Contract>();

        public override void _Ready()
        {
            if (GameManager.Instance != null)
                _contractService = new ContractService(GameManager.Instance.Player);

            LoadContracts();
            Refresh();
        }

        // ── Contract loading ──────────────────────────────────────────────────

        private void LoadContracts()
        {
            const string path = "res://resources/contracts";
            using var dir = DirAccess.Open(path);
            if (dir == null) return;

            dir.ListDirBegin();
            string name;
            while ((name = dir.GetNext()) != string.Empty)
            {
                if (!name.EndsWith(".tres", System.StringComparison.OrdinalIgnoreCase)) continue;
                var so = GD.Load<HackYourWay.Data.ContractSO>($"{path}/{name}");
                if (so != null) _allContracts.Add(so.ToModel());
            }
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
            for (int i = _contractListParent.GetChildCount() - 1; i >= 0; i--)
                _contractListParent.GetChild(i).QueueFree();

            var available = _contractService.GetAvailable(_allContracts);

            for (int i = 0; i < available.Count; i++)
            {
                Contract contract = available[i];
                var row = _contractRowPrefab.Instantiate();
                _contractListParent.AddChild(row);

                var descLabel   = row.GetNodeOrNull<Label>("DescriptionLabel");
                var rewardLabel = row.GetNodeOrNull<Label>("RewardLabel");
                if (descLabel   != null) descLabel.Text   = contract.Description;
                if (rewardLabel != null) rewardLabel.Text = $"Reward: {contract.Reward:F4} {contract.RewardCurrency}";

                var btn = row.GetNodeOrNull<Button>("AcceptButton");
                if (btn != null)
                {
                    Contract captured = contract;
                    btn.Pressed += () => OnAccept(captured);
                }
            }

            // Also display active contracts.
            if (GameManager.Instance != null)
            {
                var active = GameManager.Instance.Player.ActiveContracts;
                for (int i = 0; i < active.Count; i++)
                {
                    var row = _contractRowPrefab.Instantiate();
                    _contractListParent.AddChild(row);
                    var descLabel = row.GetNodeOrNull<Label>("DescriptionLabel");
                    if (descLabel != null)
                        descLabel.Text = $"[ACTIVE] {active[i].Description}";
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
