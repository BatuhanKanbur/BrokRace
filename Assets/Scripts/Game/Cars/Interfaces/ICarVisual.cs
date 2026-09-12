using UnityEngine;

namespace Game.Cars.Interfaces
{
    public interface ICarVisual
    {
        public void Paint(Color color);
        public void PlayBoost(int level, Color color);
        public void StopBoost();
        public void Present(float speed, float deltaTime);
        public void Reset();
    }
}
