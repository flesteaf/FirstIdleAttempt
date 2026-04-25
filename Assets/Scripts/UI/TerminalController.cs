using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using HackYourWay.Core;
using HackYourWay.Models;

namespace HackYourWay.UI
{
    /// <summary>
    /// Wires a TMP InputField to the <see cref="CommandParser"/>.
    /// Passes command results to <see cref="TerminalOutputView"/> and clears
    /// the input field after each submission.
    /// Supports interactive selection mode (<see cref="AwaitSelection"/>) for
    /// arrow-key-navigable numbered lists used by inject, forget, firewall, ls, copy.
    /// </summary>
    public class TerminalController : MonoBehaviour
    {
        public static TerminalController Instance { get; private set; }

        [SerializeField] private TMP_InputField    _inputField;
        [SerializeField] private TerminalOutputView _outputView;

        private readonly CommandHistory _history = new CommandHistory();

        // ── Selection state ───────────────────────────────────────────────────

        private struct SelectionState
        {
            public string[]           Options;       // includes "Cancel" as last entry
            public int                HighlightIndex;
            public System.Action<int> OnSelected;
        }

        private SelectionState? _selectionState;
        private int             _selectionCheckpoint = -1;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            if (_inputField != null)
            {
                _inputField.onSubmit.AddListener(OnSubmit);
                _inputField.ActivateInputField();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_inputField != null)
                _inputField.onSubmit.RemoveListener(OnSubmit);
        }

        private void Update()
        {
            if (_inputField == null || !_inputField.isFocused) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (_selectionState != null)
            {
                if (kb.upArrowKey.wasPressedThisFrame)   MoveHighlight(-1);
                if (kb.downArrowKey.wasPressedThisFrame) MoveHighlight(+1);
                return; // suppress history navigation during selection
            }

            if (kb.tabKey.wasPressedThisFrame)       HandleTab();
            if (kb.upArrowKey.wasPressedThisFrame)   HandleHistoryUp();
            if (kb.downArrowKey.wasPressedThisFrame) HandleHistoryDown();
        }

        // ── Command submission ────────────────────────────────────────────────

        private void OnSubmit(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                if (_selectionState != null)
                {
                    // Empty Enter confirms current highlight
                    ConfirmSelection();
                    _inputField.text = string.Empty;
                    _inputField.ActivateInputField();
                }
                return;
            }

            if (_selectionState != null)
            {
                HandleSelectionInput(input.Trim());
                _inputField.text = string.Empty;
                _inputField.ActivateInputField();
                return;
            }

            _history.Add(input.Trim());
            _outputView?.AppendLine($"› {input}", TerminalLineType.Command);

            if (GameManager.Instance != null)
            {
                var result = GameManager.Instance.CommandParser.Parse(input);
                if (!string.IsNullOrEmpty(result.Message))
                    _outputView?.AppendLine(result.Message, TerminalLineType.Output);
            }
            else
            {
                _outputView?.AppendLine("Error: GameManager not initialised.", TerminalLineType.Error);
            }

            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

        // ── Selection mode public API ─────────────────────────────────────────

        /// <summary>
        /// Enters interactive selection mode. "Cancel" is automatically appended as the last option.
        /// <paramref name="onSelected"/> is called with the zero-based index into <paramref name="options"/>
        /// on confirm, or -1 on cancel/free-text abort.
        /// </summary>
        public void AwaitSelection(string[] options, System.Action<int> onSelected)
        {
            var allOptions = new string[options.Length + 1];
            for (int i = 0; i < options.Length; i++) allOptions[i] = options[i];
            allOptions[options.Length] = "Cancel";

            _selectionState = new SelectionState
            {
                Options        = allOptions,
                HighlightIndex = 0,
                OnSelected     = onSelected
            };

            _selectionCheckpoint = _outputView?.SaveCheckpoint() ?? -1;
            RenderSelection();
        }

        /// <summary>Appends a line to the terminal output (used by command callbacks).</summary>
        public void AppendOutput(string message)
        {
            if (!string.IsNullOrEmpty(message))
                _outputView?.AppendLine(message);
        }

        // ── Selection internals ───────────────────────────────────────────────

        private void HandleSelectionInput(string input)
        {
            var state = _selectionState!.Value;

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= state.Options.Length)
            {
                // Jump to chosen row then confirm
                state.HighlightIndex = choice - 1;
                _selectionState      = state;
                RenderSelection();
                ConfirmSelection();
            }
            else
            {
                // Non-digit or out-of-range → cancel
                CancelSelection();
            }
        }

        private void MoveHighlight(int delta)
        {
            if (_selectionState == null) return;
            var state = _selectionState.Value;
            int next  = state.HighlightIndex + delta;
            if (next < 0 || next >= state.Options.Length) return;
            state.HighlightIndex = next;
            _selectionState      = state;
            RenderSelection();
        }

        private void ConfirmSelection()
        {
            if (_selectionState == null) return;
            var state    = _selectionState.Value;
            bool isCancel = state.HighlightIndex == state.Options.Length - 1;
            ClearSelection();
            state.OnSelected(isCancel ? -1 : state.HighlightIndex);
        }

        private void CancelSelection()
        {
            if (_selectionState == null) return;
            var onSelected = _selectionState.Value.OnSelected;
            ClearSelection();
            onSelected(-1);
        }

        private void ClearSelection()
        {
            _selectionState      = null;
            _selectionCheckpoint = -1;
        }

        private void RenderSelection()
        {
            if (_selectionState == null || _outputView == null) return;
            var state = _selectionState.Value;

            if (_selectionCheckpoint >= 0)
                _outputView.RestoreToCheckpoint(_selectionCheckpoint);

            for (int i = 0; i < state.Options.Length; i++)
            {
                string prefix = i == state.HighlightIndex ? "> " : "  ";
                _outputView.AppendLineNoFlush($"{prefix}{i + 1}. {state.Options[i]}");
            }

            _outputView.FlushNow();
        }

        // ── Autocomplete ──────────────────────────────────────────────────────

        private void HandleTab()
        {
            if (GameManager.Instance == null) return;

            string prefix  = _inputField.text.Trim();
            var    verbs   = GameManager.Instance.CommandParser.GetRegisteredVerbs();
            var    matches = new System.Collections.Generic.List<string>();

            for (int i = 0; i < verbs.Count; i++)
            {
                if (verbs[i].StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                    matches.Add(verbs[i]);
            }

            if (matches.Count == 1)
            {
                _inputField.text = matches[0];
                _inputField.caretPosition = matches[0].Length;
            }
            else if (matches.Count > 1)
            {
                _outputView?.AppendLine(string.Join("  ", matches));
            }

            EventSystem.current?.SetSelectedGameObject(null);
            _inputField.ActivateInputField();
        }

        // ── Command history ───────────────────────────────────────────────────

        private void HandleHistoryUp()
        {
            string entry = _history.NavigateBack();
            if (entry == null) return;
            _inputField.text = entry;
            _inputField.caretPosition = entry.Length;
        }

        private void HandleHistoryDown()
        {
            string entry = _history.NavigateForward();
            _inputField.text          = entry ?? string.Empty;
            _inputField.caretPosition = _inputField.text.Length;
        }
    }
}
