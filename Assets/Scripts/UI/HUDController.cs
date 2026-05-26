using Godot;
using HackYourWay.Core;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.UI
{
    /// <summary>
    /// Heads-up display showing the player's current currency balance and active location.
    /// Implements <see cref="ITickable"/> so it updates every game tick rather than every frame
    /// (Constitution Principle IV — no per-frame _Process overhead).
    /// </summary>
    public partial class HUDController : Control, ITickable
    {
        [Export] private Label _bitcoinLabel;
        [Export] private Label _locationLabel;

        public override void _Ready()
        {
            if (TickManager.Instance != null)
                TickManager.Instance.Register(this);

            Refresh();
        }

        public override void _ExitTree()
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

        private double _lastBtc          = double.MinValue;
        private string _lastLocationName = null;

        private void Refresh()
        {
            if (GameManager.Instance == null) return;

            double btc = GameManager.Instance.Player.GetBalance(CurrencyType.Bitcoin);
            if (btc != _lastBtc)
            {
                _lastBtc = btc;
                if (_bitcoinLabel != null)
                    _bitcoinLabel.Text = $"BTC: {btc:F4}";
            }

            string locationName = GameManager.Instance.LocationService?.GetCurrentLocationName() ?? string.Empty;
            if (locationName != _lastLocationName)
            {
                _lastLocationName = locationName;
                if (_locationLabel != null)
                    _locationLabel.Text = string.IsNullOrEmpty(locationName) ? "" : $"LOC: {locationName}";
            }
        }
    }
}
