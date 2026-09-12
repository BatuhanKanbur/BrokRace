using System;
using Game.Boost.Interfaces;
using Game.Cars.Structure;

namespace Game.Cars.Interfaces
{
    public interface ICar : ICarProgress
    {
        public event Action<ICar, float> OnCrossedFinish;
        public IBoostController Boost { get; }
        public IBoostState BoostState { get; }
        public IBoostLedger Ledger { get; }
        public float BalanceScale { get; }
        public void Initialize(CarSetup setup);
        public void Step(float stepTime);
        public void Present(float deltaTime);
        public void SetBalanceScale(float scale);
        public void OpenBoostWindow();
        public void CloseBoostWindow();
        public void MarkFinished(int order, float time);
        public void Release();
    }
}
