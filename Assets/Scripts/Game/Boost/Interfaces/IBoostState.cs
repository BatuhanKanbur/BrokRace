namespace Game.Boost.Interfaces
{
    public interface IBoostState
    {
        public bool IsActive { get; }
        public int ActiveLevel { get; }
        public float Multiplier { get; }
        public float RemainingWindow { get; }
        public float RemainingCooldown { get; }
        public float Energy { get; }
        public float EnergyRatio { get; }
        public float CostOf(int level);
        public bool CanAfford(int level);
    }
}
