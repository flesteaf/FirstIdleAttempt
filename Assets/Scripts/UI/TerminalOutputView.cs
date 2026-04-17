using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HackYourWay.UI
{
    /// <summary>
    /// Line category — drives the colour applied via TMP rich-text tags.
    /// </summary>
    public enum TerminalLineType
    {
        /// <summary>Boot messages, separators, status lines.</summary>
        System,
        /// <summary>Player-typed commands echoed back (the "› cmd" line).</summary>
        Command,
        /// <summary>Normal command output.</summary>
        Output,
        /// <summary>Error / warning messages.</summary>
        Error,
    }

    /// <summary>
    /// Scrollable terminal output panel backed by a single cached <see cref="StringBuilder"/>.
    /// Uses one string assignment to the TMP text component per <see cref="AppendLine"/>
    /// call — no intermediate allocations (Constitution Principle IV).
    /// </summary>
    public class TerminalOutputView : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [SerializeField] private TextMeshProUGUI _outputText;
        [SerializeField] private ScrollRect      _scrollRect;

        [Header("Node Info")]
        [Tooltip("Node identifier shown in the boot header and the header bar.")]
        [SerializeField] private string _nodeId = "NODE_47";
        [Tooltip("Optional TMP label in the header bar — kept in sync with Node Id.")]
        [SerializeField] private TextMeshProUGUI _headerLabel;

        [Header("Colours")]
        [SerializeField] private Color _systemColor  = new Color(0.40f, 0.85f, 0.40f); // soft green
        [SerializeField] private Color _commandColor = new Color(0.00f, 1.00f, 0.25f); // bright green
        [SerializeField] private Color _outputColor  = new Color(0.85f, 0.95f, 0.85f); // near-white
        [SerializeField] private Color _errorColor   = new Color(1.00f, 0.30f, 0.30f); // red

        // ── Private state ─────────────────────────────────────────────────────

        // Cached builder — never reallocated after initialization.
        private readonly StringBuilder _buffer = new StringBuilder(4096);

        // Guards against stacking multiple end-of-frame scroll coroutines (Constitution Principle IV).
        private bool _scrollPending;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            if (_headerLabel != null)
                _headerLabel.text = $"HACK YOUR WAY  ·  {_nodeId}";

            PrintBoot();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Appends one line of text with the colour that matches <paramref name="type"/>
        /// and scrolls to the bottom.
        /// </summary>
        public void AppendLine(string line, TerminalLineType type = TerminalLineType.Output)
        {
            string hex = ColorUtility.ToHtmlStringRGB(ColorForType(type));
            _buffer.Append("<color=#").Append(hex).Append('>')
                   .Append(line)
                   .AppendLine("</color>");
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

        private Color ColorForType(TerminalLineType type) => type switch
        {
            TerminalLineType.System  => _systemColor,
            TerminalLineType.Command => _commandColor,
            TerminalLineType.Error   => _errorColor,
            _                        => _outputColor,
        };

        private void PrintBoot()
        {
            AppendLine("╔══════════════════════════════════════════╗", TerminalLineType.System);
            AppendLine($"║  HACK YOUR WAY  ·  {_nodeId,-22}║", TerminalLineType.System);
            AppendLine("╚══════════════════════════════════════════╝", TerminalLineType.System);
            AppendLine("",                                             TerminalLineType.System);
            AppendLine("  Connection established.",                    TerminalLineType.System);
            AppendLine("  Type 'help' for available commands.",        TerminalLineType.System);
            AppendLine("",                                             TerminalLineType.System);
        }

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
