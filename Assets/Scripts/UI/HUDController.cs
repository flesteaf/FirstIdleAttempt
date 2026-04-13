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

        private void Refresh()
        {
            if (GameManager.Instance == null) return;

            Player player = GameManager.Instance.Player;

            if (_bitcoinLabel != null)
            {
                double btc = player.GetBalance(CurrencyType.Bitcoin);
                _bitcoinLabel.text = $"BTC: {btc:F6}";
            }
        }
    }
}
