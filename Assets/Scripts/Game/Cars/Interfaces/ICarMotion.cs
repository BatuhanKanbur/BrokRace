using Game.Cars.Structure;

namespace Game.Cars.Interfaces
{
    public interface ICarMotion
    {
        public float Distance { get; }
        public float Speed { get; }
        public float BaseSpeed { get; }
        public float BalanceScale { get; }
        public MotionStep Advance(float stepTime);
        public void SetBalanceScale(float scale);
        public void Reset();
    }
}
