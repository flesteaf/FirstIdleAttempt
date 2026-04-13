using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HackYourWay.Core;
using HackYourWay.Data;
using HackYourWay.Services;

namespace HackYourWay.UI
{
    /// <summary>
    /// Renders the store catalog and handles purchase button events.
    /// Loads <see cref="StoreItemSO"/> assets from <c>Assets/Resources/StoreItems/</c>.
    /// Delegates purchases to <see cref="StoreService"/>.
    /// Feedback messages are sent to the <see cref="TerminalOutputView"/>.
    /// </summary>
    public class StoreController : MonoBehaviour
    {
        [SerializeField] private Transform          _itemListParent;
        [SerializeField] private GameObject         _itemRowPrefab;
        [SerializeField] private TerminalOutputView _terminalOutput;

        private StoreService _storeService;

        private void Start()
        {
            if (GameManager.Instance != null)
                _storeService = new StoreService(GameManager.Instance.Player);

            BuildCatalog();
        }

        // ── Catalog rendering ─────────────────────────────────────────────────

        private void BuildCatalog()
        {
            if (_itemListParent == null || _itemRowPrefab == null) return;

            // Clear existing rows.
            for (int i = _itemListParent.childCount - 1; i >= 0; i--)
                Destroy(_itemListParent.GetChild(i).gameObject);

            StoreItemSO[] items = Resources.LoadAll<StoreItemSO>("StoreItems");

            for (int i = 0; i < items.Length; i++)
            {
                StoreItemSO so  = items[i];
                GameObject  row = Instantiate(_itemRowPrefab, _itemListParent);

                // Set display name and price via child TMP labels.
                var labels = row.GetComponentsInChildren<TextMeshProUGUI>();
                if (labels.Length >= 2)
                {
                    labels[0].text = so.DisplayName;
                    labels[1].text = $"{so.Price:F4} {so.PriceCurrency}";
                }

                // Wire purchase button.
                var btn = row.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    StoreItemSO captured = so; // capture for closure
                    btn.onClick.AddListener(() => OnPurchase(captured));
                }
            }
        }

        // ── Purchase handler ──────────────────────────────────────────────────

        private void OnPurchase(StoreItemSO so)
        {
            if (_storeService == null) return;

            var result = _storeService.Purchase(so.ToModel());
            _terminalOutput?.AppendLine(result.Message);

            // Rebuild catalog to refresh UI state (bought items can be greyed out in future).
            BuildCatalog();
        }
    }
}
