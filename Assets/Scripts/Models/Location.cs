using System.Collections.Generic;

namespace HackYourWay.Models
{
    /// <summary>A virtual area containing discoverable networks.</summary>
    [System.Serializable]
    public class Location
    {
        /// <summary>Unique identifier; also used as PRNG seed for procedural generation.</summary>
        public string Id;

        /// <summary>Reference to the LocationConfigSO asset id used for generation.</summary>
        public string ConfigId;

        /// <summary>All networks in this location, populated on first visit.</summary>
        public List<Network> Networks = new List<Network>();

        /// <summary>True after the player has run <c>scan</c> here at least once.</summary>
        public bool IsVisited;
    }
}
