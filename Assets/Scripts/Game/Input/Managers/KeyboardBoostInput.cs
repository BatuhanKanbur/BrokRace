using System;
using System.Collections.Generic;
using Game.Input.Interfaces;
using UnityEngine.InputSystem;
using static Game.Boost.Constants.BoostConstants;

namespace Game.Input.Managers
{
    public class KeyboardBoostInput : IBoostInputSource
    {
        private static readonly Key[] DigitRow =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5
        };
        private static readonly Key[] NumpadRow =
        {
            Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5
        };

        private readonly List<int> _pending = new();

        public event Action<int> OnBoostRequested;

        public void Poll()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            for (var index = 0; index < LevelCount; index++)
            {
                if (keyboard[DigitRow[index]].wasPressedThisFrame || keyboard[NumpadRow[index]].wasPressedThisFrame)
                    _pending.Add(index + MinLevel);
            }
        }

        public void Sample(float raceTime)
        {
            foreach (var level in _pending)
                OnBoostRequested?.Invoke(level);
            _pending.Clear();
        }

        public void Reset() => _pending.Clear();
    }
}
