using System;

namespace Game.Input.Interfaces
{
    public interface IBoostInputSource
    {
        public event Action<int> OnBoostRequested;
        public void Poll();
        public void Sample(float raceTime);
        public void Reset();
    }
}
