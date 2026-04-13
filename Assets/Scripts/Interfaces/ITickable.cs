namespace HackYourWay.Interfaces
{
    /// <summary>Implemented by any system that needs to receive a periodic game tick.</summary>
    public interface ITickable
    {
        /// <summary>Called once per tick by <see cref="HackYourWay.Core.TickManager"/>.</summary>
        /// <param name="deltaSeconds">Seconds elapsed since the previous tick (normally 1.0).</param>
        void OnTick(double deltaSeconds);
    }
}
