using System.Text;
using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>ls</c> — lists files on the currently targeted device.
    /// Uses a cached <see cref="StringBuilder"/> to avoid per-call GC allocations.
    /// </summary>
    public class LsCommand : ICommand
    {
        private readonly Player _player;
        private readonly StringBuilder _sb = new StringBuilder(1024);

        public LsCommand(Player player)
        {
            _player = player;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (_player.TargetedDevice == null)
                return CommandResult.Fail("No device targeted. Run 'scan ip {IP}' first.");

            Device dev = _player.TargetedDevice;

            _sb.Clear();
            _sb.AppendLine($"Files on {dev.Ip}:");

            if (dev.Files.Count == 0)
            {
                _sb.AppendLine("  (no files found)");
                return CommandResult.Ok(_sb.ToString());
            }

            for (int i = 0; i < dev.Files.Count; i++)
            {
                DeviceFile f    = dev.Files[i];
                string     size = FormatSize(f.SizeBytes);
                _sb.AppendLine($"  {f.Path,-42} ({size})");
            }

            return CommandResult.Ok(_sb.ToString());
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
            if (bytes >= 1024L * 1024)        return $"{bytes / (1024.0 * 1024):F1} MB";
            if (bytes >= 1024L)               return $"{bytes / 1024.0:F1} KB";
            return $"{bytes} B";
        }
    }
}
