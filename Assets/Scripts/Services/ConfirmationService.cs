using System;

namespace HackYourWay.Services
{
    /// <summary>
    /// Pure C# state machine managing one in-flight yes/no confirmation at a time.
    /// No Unity dependencies — fully testable in edit mode.
    /// Constructed in <see cref="Core.GameManager"/> and injected into commands that need multi-turn interaction.
    /// </summary>
    public class ConfirmationService
    {
        private struct PendingConfirmation
        {
            public string Prompt;
            public Action OnConfirm;
            public Action OnCancel;
        }

        private PendingConfirmation? _pending;

        /// <summary>True when a command is waiting for a y/n response.</summary>
        public bool IsPending => _pending.HasValue;

        /// <summary>The prompt text displayed to the player; <c>null</c> when not pending.</summary>
        public string PendingPrompt => _pending?.Prompt;

        /// <summary>
        /// Registers a pending confirmation, replacing any existing one without invoking its callbacks.
        /// </summary>
        public void RequestConfirmation(string prompt, Action onConfirm, Action onCancel)
        {
            _pending = new PendingConfirmation { Prompt = prompt, OnConfirm = onConfirm, OnCancel = onCancel };
        }

        /// <summary>
        /// Resolves the pending confirmation: invokes <c>onConfirm</c> when <paramref name="confirmed"/> is
        /// <c>true</c>, otherwise <c>onCancel</c>. Clears pending state regardless.
        /// </summary>
        public void Resolve(bool confirmed)
        {
            if (_pending == null) return;
            PendingConfirmation p = _pending.Value;
            _pending = null;
            if (confirmed)
                p.OnConfirm?.Invoke();
            else
                p.OnCancel?.Invoke();
        }

        /// <summary>Clears pending state without invoking either callback.</summary>
        public void Cancel()
        {
            _pending = null;
        }
    }
}
