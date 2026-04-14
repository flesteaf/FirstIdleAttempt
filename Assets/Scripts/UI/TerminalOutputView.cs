using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HackYourWay.UI
{
    /// <summary>
    /// Scrollable terminal output panel backed by a single cached <see cref="StringBuilder"/>.
    /// Uses one string assignment to the TMP text component per <see cref="AppendLine"/>
    /// call — no intermediate allocations (Constitution Principle IV).
    /// </summary>
    public class TerminalOutputView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _outputText;
        [SerializeField] private ScrollRect      _scrollRect;

        // Cached builder — never reallocated after initialization.
        private readonly StringBuilder _buffer = new StringBuilder(4096);

        // Guards against stacking multiple end-of-frame scroll coroutines (Constitution Principle IV).
        private bool _scrollPending;

        /// <summary>Appends one line of text to the terminal and scrolls to bottom.</summary>
        public void AppendLine(string line)
        {
            _buffer.AppendLine(line);
            Flush();
        }

        /// <summary>Clears all terminal output.</summary>
        public void Clear()
        {
            _buffer.Clear();
            if (_outputText != null)
                _outputText.text = string.Empty;
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void Flush()
        {
            if (_outputText != null)
                _outputText.text = _buffer.ToString();

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (_scrollRect == null || _scrollPending) return;

            // Defer to end-of-frame so the layout rebuild for the new text completes
            // before we set the scroll position. A pending guard prevents stacking
            // multiple coroutines when several AppendLine calls arrive in one frame.
            _scrollPending = true;
            StartCoroutine(ScrollAtEndOfFrame());
        }

        private IEnumerator ScrollAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            _scrollRect.verticalNormalizedPosition = 0f;
            _scrollPending = false;
        }
    }
}
