using System.Collections.Generic;

namespace HackYourWay.Models
{
    /// <summary>
    /// Session-bound command history with Up/Down navigation.
    /// Capped at <see cref="MaxEntries"/> to prevent unbounded memory growth.
    /// History is not persisted across game sessions.
    /// </summary>
    public class CommandHistory
    {
        /// <summary>Maximum number of history entries retained per session.</summary>
        public const int MaxEntries = 50;

        private readonly List<string> _entries = new List<string>(MaxEntries + 1);

        // -1 means "not navigating" (fresh state, no active browse).
        private int _navIndex = -1;

        /// <summary>
        /// Appends a command to history. Drops the oldest entry when the cap is
        /// exceeded. Resets the navigation pointer to the fresh state.
        /// </summary>
        public void Add(string command)
        {
            _entries.Add(command);

            if (_entries.Count > MaxEntries)
                _entries.RemoveAt(0);

            _navIndex = -1;
        }

        /// <summary>
        /// Moves the navigation pointer one step toward the oldest entry and
        /// returns the entry at that position. Returns <c>null</c> if history is empty.
        /// Does not go below index 0 — calling at the oldest entry returns it again.
        /// </summary>
        public string NavigateBack()
        {
            if (_entries.Count == 0)
                return null;

            if (_navIndex == -1)
                _navIndex = _entries.Count - 1;
            else if (_navIndex > 0)
                _navIndex--;

            return _entries[_navIndex];
        }

        /// <summary>
        /// Moves the navigation pointer one step toward the newest entry.
        /// Returns the entry at the new position, or <c>null</c> when the pointer
        /// steps past the newest entry — which signals the caller to clear the
        /// input field and reset the browse session.
        /// </summary>
        public string NavigateForward()
        {
            if (_navIndex == -1)
                return null;

            _navIndex++;

            if (_navIndex >= _entries.Count)
            {
                _navIndex = -1;
                return null;
            }

            return _entries[_navIndex];
        }

        /// <summary>
        /// Resets the navigation pointer without clearing history entries.
        /// Call when the player starts typing a new character mid-browse.
        /// </summary>
        public void ResetNavigation()
        {
            _navIndex = -1;
        }
    }
}
