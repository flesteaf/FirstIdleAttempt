using HackYourWay.Interfaces;
using HackYourWay.Models;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>copy {filename}</c> — copies a file from the targeted device
    /// to <see cref="Player.CopiedFiles"/>.
    /// </summary>
    public class CopyCommand : ICommand
    {
        private readonly Player _player;

        public CopyCommand(Player player)
        {
            _player = player;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (_player.TargetedDevice == null)
                return CommandResult.Fail("No device targeted. Run 'scan ip {IP}' first.");

            if (args.Length < 1)
                return CommandResult.Fail("Usage: copy {filename}");

            string query  = args[0];
            Device dev    = _player.TargetedDevice;

            DeviceFile found = null;
            for (int i = 0; i < dev.Files.Count; i++)
            {
                var f = dev.Files[i];
                // Match on full path or just the filename.
                if (f.Path == query || f.Name == query)
                {
                    found = f;
                    break;
                }
            }

            if (found == null)
                return CommandResult.Fail($"File '{query}' not found on {dev.Ip}.");

            if (!_player.CopiedFiles.Contains(found.Path))
                _player.CopiedFiles.Add(found.Path);

            return CommandResult.Ok($"Copying {found.Path}...\nFile copied to your storage.");
        }
    }
}
