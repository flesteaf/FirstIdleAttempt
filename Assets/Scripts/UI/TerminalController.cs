using Godot;
using HackYourWay.Core;
using HackYourWay.Models;

namespace HackYourWay.UI
{
    /// <summary>
    /// Wires a <see cref="LineEdit"/> to the <see cref="CommandParser"/>.
    /// Passes command results to <see cref="TerminalOutputView"/> and clears
    /// the input field after each submission.
    /// Supports interactive selection mode (<see cref="AwaitSelection"/>) for
    /// arrow-key-navigable numbered lists used by inject, forget, firewall, ls, copy.
    /// </summary>
    public partial class TerminalController : Control
    {
        public static TerminalController Instance { get; private set; }

        [Export] private LineEdit           _inputField;
        [Export] private TerminalOutputView _outputView;

        private readonly CommandHistory _history = new CommandHistory();

        // ── Selection state ───────────────────────────────────────────────────

        private struct SelectionState
        {
            public string[]           Options;
            public int                HighlightIndex;
            public System.Action<int> OnSelected;
        }

        private SelectionState? _selectionState;
        private int             _selectionCheckpoint = -1;

        // ── Command execution delay (replaces coroutine) ──────────────────────

        private float  _executionElapsed   = -1f;
        private float  _executionTotal;
        private string _executingInput;
        private int    _executionCheckpoint = -1;

        private const float InstantThreshold = 0.05f;
        private const int   ProgressBarWidth  = 20;

        // ── Godot lifecycle ───────────────────────────────────────────────────

        public override void _Ready()
        {
            if (Instance == null) Instance = this;

            if (_inputField != null)
            {
                _inputField.TextSubmitted += OnSubmit;
                _inputField.GrabFocus();
            }
        }

        public override void _ExitTree()
        {
            if (Instance == this) Instance = null;
            if (_inputField != null)
                _inputField.TextSubmitted -= OnSubmit;
        }

        public override void _Process(double delta)
        {
            if (_executionElapsed < 0) return;

            _executionElapsed += (float)delta;
            float t = Mathf.Clamp(_executionElapsed / _executionTotal, 0f, 1f);

            if (_executionCheckpoint >= 0 && _outputView != null)
            {
                int filled = Mathf.RoundToInt(t * ProgressBarWidth);
                _outputView.RestoreToCheckpoint(_executionCheckpoint);
                _outputView.AppendLineNoFlush(
                    $"  [{new string('█', filled)}{new string('░', ProgressBarWidth - filled)}] {(int)(t * 100),3}%",
                    TerminalLineType.System);
                _outputView.FlushNow();
            }

            if (_executionElapsed >= _executionTotal)
            {
                if (_executionCheckpoint >= 0 && _outputView != null)
                    _outputView.RestoreToCheckpoint(_executionCheckpoint);

                string input      = _executingInput;
                _executionElapsed = -1f;
                _executingInput   = null;
                ExecuteCommandImmediate(input);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is not InputEventKey key || !key.Pressed || key.Echo) return;
            if (_inputField == null || !_inputField.HasFocus()) return;

            if (_selectionState != null)
            {
                if (key.Keycode == Key.Up)   { MoveHighlight(-1); GetViewport().SetInputAsHandled(); }
                if (key.Keycode == Key.Down) { MoveHighlight(+1); GetViewport().SetInputAsHandled(); }
                return;
            }

            if (key.Keycode == Key.Tab)
            {
                HandleTab();
                GetViewport().SetInputAsHandled();
            }
            else if (key.Keycode == Key.Up)
            {
                HandleHistoryUp();
                GetViewport().SetInputAsHandled();
            }
            else if (key.Keycode == Key.Down)
            {
                HandleHistoryDown();
                GetViewport().SetInputAsHandled();
            }
        }

        // ── Command submission ────────────────────────────────────────────────

        private void OnSubmit(string input)
        {
            _inputField.Text = string.Empty;
            _inputField.GrabFocus();

            if (string.IsNullOrWhiteSpace(input))
            {
                if (_selectionState != null)
                {
                    ConfirmSelection();
                }
                return;
            }

            if (_selectionState != null)
            {
                HandleSelectionInput(input.Trim());
                return;
            }

            // ── Confirmation routing ──────────────────────────────────────────
            var confirmSvc = GameManager.Instance?.ConfirmationService;
            if (confirmSvc != null && confirmSvc.IsPending)
            {
                _history.Add(input.Trim());
                _outputView?.AppendLine($"› {input}", TerminalLineType.Command);
                confirmSvc.Resolve(input.Trim().ToLower() == "y");
                return;
            }

            // Drop input silently while a command is already executing
            if (_executionElapsed >= 0) return;

            _history.Add(input.Trim());
            _outputView?.AppendLine($"› {input}", TerminalLineType.Command);

            float latency = 0f;
            if (GameManager.Instance != null)
            {
                string[] tokens = input.Trim().Split(' ');
                string   verb   = tokens[0].ToLowerInvariant();
                var      cmd    = GameManager.Instance.CommandParser.GetCommand(verb);
                if (cmd != null)
                {
                    int      argCount = tokens.Length - 1;
                    string[] args     = argCount > 0 ? new string[argCount] : System.Array.Empty<string>();
                    for (int i = 0; i < argCount; i++) args[i] = tokens[i + 1];
                    latency = cmd.GetLatency(args);
                }
            }

            if (latency <= InstantThreshold)
            {
                ExecuteCommandImmediate(input.Trim());
            }
            else
            {
                _executionCheckpoint = _outputView?.SaveCheckpoint() ?? -1;
                _executionTotal      = latency;
                _executionElapsed    = 0f;
                _executingInput      = input.Trim();
            }
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

        // ── Command execution ─────────────────────────────────────────────────

        private void ExecuteCommandImmediate(string input)
        {
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
        }

        // ── Selection internals ───────────────────────────────────────────────

        private void HandleSelectionInput(string input)
        {
            var state = _selectionState!.Value;

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= state.Options.Length)
            {
                state.HighlightIndex = choice - 1;
                _selectionState      = state;
                RenderSelection();
                ConfirmSelection();
            }
            else
            {
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

            string prefix  = _inputField.Text.Trim();
            var    verbs   = GameManager.Instance.CommandParser.GetRegisteredVerbs();
            var    matches = new System.Collections.Generic.List<string>();

            for (int i = 0; i < verbs.Count; i++)
            {
                if (verbs[i].StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                    matches.Add(verbs[i]);
            }

            if (matches.Count == 1)
            {
                _inputField.Text         = matches[0];
                _inputField.CaretColumn  = matches[0].Length;
            }
            else if (matches.Count > 1)
            {
                _outputView?.AppendLine(string.Join("  ", matches));
            }

            _inputField.GrabFocus();
        }

        // ── Command history ───────────────────────────────────────────────────

        private void HandleHistoryUp()
        {
            string entry = _history.NavigateBack();
            if (entry == null) return;
            _inputField.Text        = entry;
            _inputField.CaretColumn = entry.Length;
        }

        private void HandleHistoryDown()
        {
            string entry            = _history.NavigateForward();
            _inputField.Text        = entry ?? string.Empty;
            _inputField.CaretColumn = _inputField.Text.Length;
        }
    }
}
