using System;
using Game.Boost.Enums;

namespace Game.Boost.Interfaces
{
    public interface IBoostController
    {
        public event Action<int> OnBoostStarted;
        public event Action OnBoostEnded;
        public event Action<BoostRequestOutcome> OnRequestRejected;
        public BoostRequestOutcome Request(int level);
    }
}
