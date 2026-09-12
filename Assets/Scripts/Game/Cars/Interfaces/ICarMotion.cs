using Game.Cars.Structure;

namespace Game.Cars.Interfaces
{
    public interface ICarMotion
    {
        public float Distance { get; }
        public float Speed { get; }
        public float NaturalSpeed { get; }
        public float BaseSpeed { get; }
        public float BalanceScale { get; }
        public float BaseDistance { get; }
        public float BoostDistance { get; }
        public float AssistDistance { get; }
        public void Configure(float naturalSpeed, float finishDistance);
        public MotionStep Advance(float stepTime);
        public void SetBalanceScale(float scale);
        public void Reset();
    }
}
