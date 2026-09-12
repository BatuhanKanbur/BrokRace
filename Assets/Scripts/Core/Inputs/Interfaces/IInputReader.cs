using System;
using UnityEngine;

namespace Core.Inputs.Interfaces
{
    public interface IInputReader
    {
        public event Action OnTap;
        public Vector2 GetPointerPosition();
        public void Enable();
        public void Disable();
    }
}