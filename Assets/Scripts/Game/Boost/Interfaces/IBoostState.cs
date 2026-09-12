using Game.Boost.Enums;
using Game.Configuration.Structure;

namespace Game.Boost.Interfaces
{
    public interface IBoostState
    {
        public bool IsActive { get; }
        public int ActiveLevel { get; }
        public float Multiplier { get; }
        public BoostCurve Curve { get; }
        public BoostSettings Settings { get; }
        public float WindowDuration { get; }
        public float RemainingWindow { get; }
        public float RemainingCooldown { get; }
        public float Energy { get; }
        public float Capacity { get; }
        public float EnergyRatio { get; }
        public float CostOf(int level);
        public BoostCurve CurveOf(int level);
        public bool CanAfford(int level);
    }
}
