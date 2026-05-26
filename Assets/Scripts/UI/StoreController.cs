using Godot;
using HackYourWay.Core;
using HackYourWay.Data;
using HackYourWay.Services;

namespace HackYourWay.UI
{
    /// <summary>
    /// Renders the store catalog and handles purchase button events.
    /// Loads <see cref="StoreItemSO"/> resources from <c>res://resources/store_items/</c>.
    /// Delegates purchases to <see cref="StoreService"/>.
    ///
    /// Expected row scene node layout:
    ///   StoreItemRow.tscn
    ///   ├── NameLabel  (Label)
    ///   ├── PriceLabel (Label)
    ///   └── BuyButton  (Button)
    /// </summary>
    public partial class StoreController : Control
    {
        [Export] private Node               _itemListParent;
        [Export] private PackedScene        _itemRowPrefab;
        [Export] private TerminalOutputView _terminalOutput;

        private StoreService _storeService;

        public override void _Ready()
        {
            if (GameManager.Instance != null)
                _storeService = new StoreService(GameManager.Instance.Player);

            BuildCatalog();
        }

        // ── Catalog rendering ─────────────────────────────────────────────────

        private void BuildCatalog()
        {
            if (_itemListParent == null || _itemRowPrefab == null) return;

            for (int i = _itemListParent.GetChildCount() - 1; i >= 0; i--)
                _itemListParent.GetChild(i).QueueFree();

            const string path = "res://resources/store_items";
            using var dir = DirAccess.Open(path);
            if (dir == null) return;

            dir.ListDirBegin();
            string name;
            while ((name = dir.GetNext()) != string.Empty)
            {
                if (!name.EndsWith(".tres", System.StringComparison.OrdinalIgnoreCase)) continue;
                var so = GD.Load<StoreItemSO>($"{path}/{name}");
                if (so == null) continue;

                var row = _itemRowPrefab.Instantiate();
                _itemListParent.AddChild(row);

                var nameLabel  = row.GetNodeOrNull<Label>("NameLabel");
                var priceLabel = row.GetNodeOrNull<Label>("PriceLabel");
                if (nameLabel  != null) nameLabel.Text  = so.DisplayName;
                if (priceLabel != null) priceLabel.Text = $"{so.Price:F4} {so.PriceCurrency}";

                var btn = row.GetNodeOrNull<Button>("BuyButton");
                if (btn != null)
                {
                    StoreItemSO captured = so;
                    btn.Pressed += () => OnPurchase(captured);
                }
            }
        }

        // ── Purchase handler ──────────────────────────────────────────────────

        private void OnPurchase(StoreItemSO so)
        {
            if (_storeService == null) return;

            var result = _storeService.Purchase(so.ToModel());
            _terminalOutput?.AppendLine(result.Message);
            BuildCatalog();
        }
    }
}
