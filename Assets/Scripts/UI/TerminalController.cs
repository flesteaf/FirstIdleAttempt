using TMPro;
using UnityEngine;
using HackYourWay.Core;

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

        private void OnSubmit(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

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
    }
}
