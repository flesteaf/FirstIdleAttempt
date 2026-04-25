using HackYourWay.Interfaces;

namespace HackYourWay.Services.Commands
{
    /// <summary>
    /// Implements <c>move</c> and <c>move {location-name}</c>.
    /// <c>move</c> (no args) discovers a new location; <c>move {name}</c> returns to a known one.
    /// This is the sole mechanism for changing the player's current location.
    /// </summary>
    public class MoveCommand : ICommand
    {
        private readonly LocationService _locationService;

        public MoveCommand(LocationService locationService)
        {
            _locationService = locationService;
        }

        /// <inheritdoc/>
        public CommandResult Execute(string[] args)
        {
            if (args.Length == 0)
                return MoveToNew();

            return MoveToNamed(args[0]);
        }

        private CommandResult MoveToNew()
        {
            var loc = _locationService.MoveToNextLocation();
            return CommandResult.Ok($"Moved to {loc.Name}.");
        }

        private CommandResult MoveToNamed(string name)
        {
            string currentName = _locationService.GetCurrentLocationName();

            if (currentName == name)
                return CommandResult.Ok($"Already at {name}.");

            if (_locationService.SetCurrentLocation(name))
                return CommandResult.Ok($"Moved to {name}.");

            return CommandResult.Fail(
                $"Location '{name}' not found. Use 'move' without arguments to discover a new location.");
        }
    }
}
