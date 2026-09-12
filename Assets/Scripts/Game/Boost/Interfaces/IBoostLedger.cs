namespace Game.Boost.Interfaces
{
    public interface IBoostLedger
    {
        public int AcceptedCount { get; }
        public int RejectedCount { get; }
        public float EnergySpent { get; }
        public float BoostedSeconds { get; }
    }
}
