using Game.Cars.Interfaces;

namespace Game.Balancing.Interfaces
{
    public interface IRaceBalancer
    {
        public bool IsEnabled { get; }
        public float ScaleFor(ICarProgress car);
        public void Step(float stepTime);
        public void Reset();
    }
}
