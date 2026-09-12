using System;
using Core.Inputs.Interfaces;
using UnityEngine;

namespace Core.Inputs.Managers
{
    public class InputReader : MonoBehaviour, IInputReader
    {
        public event Action OnTap;
        private bool _isActive;
        private void Update()
        {
            if (!_isActive) return;

            if (Input.GetMouseButtonDown(0))
            {
                OnTap?.Invoke();
            }
        }

        public Vector2 GetPointerPosition()
        {
            return Input.mousePosition;
        }

        public void Enable() => _isActive = true;
        public void Disable() => _isActive = false;
    }
}