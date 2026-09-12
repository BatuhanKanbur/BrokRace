using Game.Boost.Interfaces;
using Game.Cars.Structure;
using UnityEngine;

namespace Game.Cars.Interfaces
{
    public interface ICar : ICarProgress
    {
        public Transform Transform { get; }
        public IBoostController Boost { get; }
        public void Initialize(CarSetup setup);
        public MotionStep Step(float stepTime);
        public void Present(float deltaTime);
        public void SetBalanceScale(float scale);
        public void OpenBoostWindow();
        public void CloseBoostWindow();
        public void MarkFinished(int order, float time);
        public void Release();
    }
}
