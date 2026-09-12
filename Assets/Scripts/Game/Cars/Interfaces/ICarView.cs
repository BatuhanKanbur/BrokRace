using UnityEngine;

namespace Game.Cars.Interfaces
{
    public interface ICarView
    {
        public Transform Transform { get; }
        public void Present(float deltaTime);
        public void Release();
    }
}
