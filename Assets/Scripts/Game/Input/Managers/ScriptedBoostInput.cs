using System;
using System.Collections.Generic;
using Game.Input.Interfaces;
using Game.Input.Structure;

namespace Game.Input.Managers
{
    public class ScriptedBoostInput : IBoostInputSource
    {
        private readonly IReadOnlyList<ScriptedBoostEvent> _schedule;
        private int _cursor;

        public event Action<int> OnBoostRequested;

        public ScriptedBoostInput(IReadOnlyList<ScriptedBoostEvent> schedule) => _schedule = schedule;

        public void Sample(float raceTime)
        {
            while (_cursor < _schedule.Count && _schedule[_cursor].time <= raceTime)
            {
                OnBoostRequested?.Invoke(_schedule[_cursor].level);
                _cursor++;
            }
        }

        public void Reset() => _cursor = 0;
    }
}
