using Game.Boost.Interfaces;
using Game.Cars.Structure;

namespace Game.Cars.Interfaces
{
    public interface ICar : ICarProgress
    {
        public IBoostController Boost { get; }
        public void Initialize(CarSetup setup);
        public MotionStep Step(float stepTime);
        public void SetBalanceScale(float scale);
        public void OpenBoostWindow();
        public void CloseBoostWindow();
        public void MarkFinished(int order, float time);
    }
}
