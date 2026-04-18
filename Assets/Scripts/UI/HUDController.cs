using TMPro;
using UnityEngine;
using HackYourWay.Core;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.UI
{
    /// <summary>
    /// Heads-up display that shows the player's current currency balances.
    /// Implements <see cref="ITickable"/> so it updates every game tick rather
    /// than every frame (Constitution Principle IV — no per-frame Update overhead).
    /// </summary>
    public class HUDController : MonoBehaviour, ITickable
    {
        [SerializeField] private TextMeshProUGUI _bitcoinLabel;

        private void Start()
        {
            if (TickManager.Instance != null)
                TickManager.Instance.Register(this);

            Refresh(); // initial display
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

        private void Refresh()
        {
            if (GameManager.Instance == null) return;

            double btc = GameManager.Instance.Player.GetBalance(CurrencyType.Bitcoin);
            if (btc == _lastBtc) return;
            _lastBtc = btc;

            if (_bitcoinLabel != null)
                _bitcoinLabel.text = $"BTC: {btc:F4}";
        }
    }
}
