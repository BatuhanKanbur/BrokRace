using UnityEngine;

namespace Game.Cars.Interfaces
{
    public interface ICarVisual
    {
        public void Paint(Color color);
        public void PlayBoost(int level);
        public void StopBoost();
        public void Tick(float speed, float deltaTime);
        public void Reset();
    }
}
