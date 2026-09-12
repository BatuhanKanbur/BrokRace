using Game.Cars.Interfaces;

namespace Game.Balancing.Interfaces
{
    public interface IRaceBalancer
    {
        public bool IsEnabled { get; }
        public bool IsSuspended { get; }
        public float ScaleFor(ICarProgress car);
        public void Register(ICarProgress car, float response);
        public void Step(float stepTime);
        public void Reset();
    }
}
