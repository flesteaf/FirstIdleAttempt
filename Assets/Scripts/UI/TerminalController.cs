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
    /// </summary>
    public class TerminalController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField    _inputField;
        [SerializeField] private TerminalOutputView _outputView;

        private readonly CommandHistory _history = new CommandHistory();

        private void Start()
        {
            if (_inputField != null)
                _inputField.onSubmit.AddListener(OnSubmit);
        }

        private void OnDestroy()
        {
            if (_inputField != null)
                _inputField.onSubmit.RemoveListener(OnSubmit);
        }

        private void Update()
        {
            if (_inputField == null || !_inputField.isFocused) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.tabKey.wasPressedThisFrame)       HandleTab();
            if (kb.upArrowKey.wasPressedThisFrame)   HandleHistoryUp();
            if (kb.downArrowKey.wasPressedThisFrame) HandleHistoryDown();
        }

        private void OnSubmit(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

            _history.Add(input.Trim());

            // Echo the command.
            _outputView?.AppendLine($"> {input}");

            // Dispatch.
            if (GameManager.Instance != null)
            {
                var result = GameManager.Instance.CommandParser.Parse(input);
                _outputView?.AppendLine(result.Message);
            }
            else
            {
                _outputView?.AppendLine("Error: GameManager not initialised.");
            }

            // Clear and re-focus.
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

        // ── Autocomplete ──────────────────────────────────────────────────────

        /// <summary>
        /// Handles Tab key: completes a unique prefix, lists candidates for ambiguous
        /// prefixes, or lists all commands for an empty field.
        /// </summary>
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
            // 0 matches: no-op

            // Prevent EventSystem from moving focus to the next selectable.
            EventSystem.current?.SetSelectedGameObject(null);
            _inputField.ActivateInputField();
        }

        // ── Command history ───────────────────────────────────────────────────

        /// <summary>Navigates backward (Up arrow) through command history.</summary>
        private void HandleHistoryUp()
        {
            string entry = _history.NavigateBack();
            if (entry == null) return;

            _inputField.text = entry;
            _inputField.caretPosition = entry.Length;
        }

        /// <summary>
        /// Navigates forward (Down arrow) through command history.
        /// Clears the input field when past the newest entry.
        /// </summary>
        private void HandleHistoryDown()
        {
            string entry = _history.NavigateForward();
            _inputField.text          = entry ?? string.Empty;
            _inputField.caretPosition = _inputField.text.Length;
        }
    }
}
