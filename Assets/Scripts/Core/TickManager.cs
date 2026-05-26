using System.Collections.Generic;
using Godot;
using HackYourWay.Interfaces;

namespace HackYourWay.Core
{
    /// <summary>
    /// Drives all game systems that need periodic updates.
    /// Uses a <see cref="Timer"/> child node at a fixed 1-second interval —
    /// not per-frame <c>_Process</c> — to satisfy Constitution Principle IV
    /// (zero per-frame allocations in the idle tick path).
    /// </summary>
    public partial class TickManager : Node
    {
        public static TickManager Instance { get; private set; }

        private const double TickInterval = 1.0;

        private readonly List<ITickable> _subscribers = new List<ITickable>();

        public override void _Ready()
        {
            if (Instance != null && Instance != this) { QueueFree(); return; }
            Instance = this;

            var timer = new Timer();
            timer.WaitTime  = TickInterval;
            timer.Autostart = true;
            timer.Timeout   += Tick;
            AddChild(timer);
        }

        public override void _ExitTree()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Subscribes a system to receive ticks.</summary>
        public void Register(ITickable tickable)
        {
            if (!_subscribers.Contains(tickable))
                _subscribers.Add(tickable);
        }

        /// <summary>Removes a system from the tick list.</summary>
        public void Unregister(ITickable tickable)
        {
            _subscribers.Remove(tickable);
        }

        private void Tick()
        {
            for (int i = 0; i < _subscribers.Count; i++)
                _subscribers[i].OnTick(TickInterval);
        }
    }
}
