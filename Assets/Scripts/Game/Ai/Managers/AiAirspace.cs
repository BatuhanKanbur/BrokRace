using Game.Ai.Interfaces;

namespace Game.Ai.Managers
{
    public class AiAirspace : IAiAirspace
    {
        private readonly float _gate;
        private float _lastClaim;

        public AiAirspace(float gate) => _gate = gate;

        public bool TryClaim(float raceTime)
        {
            if (raceTime - _lastClaim < _gate) return false;
            _lastClaim = raceTime;
            return true;
        }

        public void Reset() => _lastClaim = float.NegativeInfinity;
    }
}
