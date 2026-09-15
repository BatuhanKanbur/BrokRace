using UnityEngine;

namespace Game.Effects.Interfaces
{
    public interface ICarEffects
    {
        public void Play(int level, Color color);
        public void Stop();
        public void Present(float speed, float intensity, float deltaTime);
        public void Celebrate(Color color);
        public void Reset();
    }
}
