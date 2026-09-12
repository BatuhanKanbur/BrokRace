using Game.Configuration.Structure;

namespace Game.Boost.Interfaces
{
    public interface IBoostClock
    {
        public void Configure(BoostSettings settings);
        public void Open();
        public void Close();
        public void Advance(float slice);
        public void Tick(float stepTime);
        public void Reset();
    }
}
