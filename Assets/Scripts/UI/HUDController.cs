using TMPro;
using UnityEngine;
using HackYourWay.Core;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.UI
{
    /// <summary>
    /// Heads-up display showing the player's current currency balance and active location.
    /// Implements <see cref="ITickable"/> so it updates every game tick rather than every frame
    /// (Constitution Principle IV — no per-frame Update overhead).
    /// </summary>
    public class HUDController : MonoBehaviour, ITickable
    {
        [SerializeField] private TextMeshProUGUI _bitcoinLabel;
        [SerializeField] private TextMeshProUGUI _locationLabel;

        private void Start()
        {
            if (TickManager.Instance != null)
                TickManager.Instance.Register(this);

            Refresh();
        }

        private void OnDestroy()
        {
            if (TickManager.Instance != null)
                TickManager.Instance.Unregister(this);
        }

        /// <inheritdoc/>
        public void OnTick(double deltaSeconds)
        {
            Refresh();
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private double _lastBtc = double.MinValue;
        private string _lastLocationName = null;

        private void Refresh()
        {
            if (GameManager.Instance == null) return;

            double btc = GameManager.Instance.Player.GetBalance(CurrencyType.Bitcoin);
            if (btc != _lastBtc)
            {
                _lastBtc = btc;
                if (_bitcoinLabel != null)
                    _bitcoinLabel.text = $"BTC: {btc:F4}";
            }

            string locationName = GameManager.Instance.LocationService?.GetCurrentLocationName() ?? string.Empty;
            if (locationName != _lastLocationName)
            {
                _lastLocationName = locationName;
                if (_locationLabel != null)
                    _locationLabel.text = string.IsNullOrEmpty(locationName) ? "" : $"LOC: {locationName}";
            }
        }
    }
}
