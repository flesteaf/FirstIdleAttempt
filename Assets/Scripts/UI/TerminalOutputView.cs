using System.Text;
using Godot;

namespace HackYourWay.UI
{
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
	/// Uses one Text assignment to the RichTextLabel per <see cref="AppendLine"/> call —
	/// no intermediate allocations (Constitution Principle IV).
	/// </summary>
	public partial class TerminalOutputView : RichTextLabel
	{
		// ── Inspector ─────────────────────────────────────────────────────────

		[Export] private RichTextLabel _outputText;

		[ExportGroup("Node Info")]
		[Export] private string _nodeId = "NODE_47";
		[Export] private Label _headerLabel;

		[ExportGroup("Colours")]
		[Export] private Color _systemColor  = new Color(0.40f, 0.85f, 0.40f);
		[Export] private Color _commandColor = new Color(0.00f, 1.00f, 0.25f);
		[Export] private Color _outputColor  = new Color(0.85f, 0.95f, 0.85f);
		[Export] private Color _errorColor   = new Color(1.00f, 0.30f, 0.30f);

		// ── Private state ─────────────────────────────────────────────────────

		private readonly StringBuilder _buffer = new StringBuilder(4096);

		// ── Godot lifecycle ───────────────────────────────────────────────────

		public override void _Ready()
		{
			if (_outputText != null)
			{
				_outputText.BbcodeEnabled  = true;
				_outputText.ScrollFollowing = true;
			}

			if (_headerLabel != null)
				_headerLabel.Text = $"HACK YOUR WAY  ·  {_nodeId}";

			PrintBoot();
		}

		// ── Public API ────────────────────────────────────────────────────────

		/// <summary>
		/// Appends one line of text with the colour that matches <paramref name="type"/>
		/// and scrolls to the bottom.
		/// </summary>
		public void AppendLine(string line, TerminalLineType type = TerminalLineType.Output)
		{
			AppendLineNoFlush(line, type);
			Flush();
		}

		/// <summary>Clears all terminal output.</summary>
		public void Clear()
		{
			_buffer.Clear();
			if (_outputText != null)
				_outputText.Text = string.Empty;
		}

		/// <summary>
		/// Returns the current buffer length as a checkpoint that can be restored
		/// to re-render the selection list in-place without growing the scrollback.
		/// </summary>
		public int SaveCheckpoint() => _buffer.Length;

		/// <summary>
		/// Truncates the buffer to <paramref name="checkpoint"/> without flushing.
		/// Call <see cref="FlushNow"/> after appending replacement lines.
		/// </summary>
		public void RestoreToCheckpoint(int checkpoint)
		{
			if (checkpoint >= 0 && checkpoint <= _buffer.Length)
				_buffer.Length = checkpoint;
		}

		/// <summary>Appends a line without flushing (batch with <see cref="FlushNow"/>).</summary>
		public void AppendLineNoFlush(string line, TerminalLineType type = TerminalLineType.Output)
		{
			string hex = ColorForType(type).ToHtml(false);
			_buffer.Append("[color=#").Append(hex).Append(']')
				   .Append(line)
				   .AppendLine("[/color]");
		}

		/// <summary>Pushes the current buffer to the RichTextLabel.</summary>
		public void FlushNow() => Flush();

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
				_outputText.Text = _buffer.ToString();
		}
	}
}
