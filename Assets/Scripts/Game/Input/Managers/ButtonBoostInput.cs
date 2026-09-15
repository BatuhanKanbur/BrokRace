using System;
using System.Collections.Generic;
using Game.Input.Interfaces;

namespace Game.Input.Managers
{
    public class ButtonBoostInput : IBoostInputSource, IBoostRequestSink
    {
        private readonly List<int> _pending = new();

        public event Action<int> OnBoostRequested;

        public void Request(int level) => _pending.Add(level);

        public void Poll() { }

        public void Sample(float raceTime)
        {
            foreach (var level in _pending)
                OnBoostRequested?.Invoke(level);
            _pending.Clear();
        }

        public void Reset() => _pending.Clear();
    }
}
