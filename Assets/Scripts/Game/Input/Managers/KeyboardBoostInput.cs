using System;
using Game.Input.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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

        public event Action<int> OnBoostRequested;
        private int _sampledFrame = -1;

        public void Sample(float raceTime)
        {
            if (_sampledFrame == Time.frameCount) return;
            _sampledFrame = Time.frameCount;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            for (var index = 0; index < DigitRow.Length; index++)
            {
                if (WasPressed(keyboard[DigitRow[index]]) || WasPressed(keyboard[NumpadRow[index]]))
                    OnBoostRequested?.Invoke(index + 1);
            }
        }

        public void Reset() => _sampledFrame = -1;

        private static bool WasPressed(KeyControl control) => control.wasPressedThisFrame;
    }
}
