using System.Collections.Generic;
using UnityEngine;
using HackYourWay.Interfaces;

namespace HackYourWay.Core
{
    /// <summary>
    /// Drives all game systems that need periodic updates.
    /// Uses <c>InvokeRepeating</c> at a fixed 1-second interval — NOT per-frame
    /// <c>Update()</c> — to satisfy Constitution Principle IV (zero per-frame
    /// allocations in the idle tick path).
    /// </summary>
    public class TickManager : MonoBehaviour
    {
        /// <summary>Singleton reference; set in Awake, cleared on destroy.</summary>
        public static TickManager Instance { get; private set; }

        private const float TickInterval = 1f;

        private readonly List<ITickable> _subscribers = new List<ITickable>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            InvokeRepeating(nameof(Tick), TickInterval, TickInterval);
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
            // No LINQ — for-loop avoids GC per Unity perf guidelines.
            for (int i = 0; i < _subscribers.Count; i++)
                _subscribers[i].OnTick(TickInterval);
        }
    }
}
