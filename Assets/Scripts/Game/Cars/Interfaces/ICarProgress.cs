using Game.Boost.Interfaces;

namespace Game.Cars.Interfaces
{
    public interface ICarProgress
    {
        public int Index { get; }
        public string DisplayName { get; }
        public bool IsPlayer { get; }
        public float Distance { get; }
        public float Speed { get; }
        public float NaturalSpeed { get; }
        public float BalanceScale { get; }
        public float BoostDistance { get; }
        public float AssistDistance { get; }
        public float Progress { get; }
        public bool HasFinished { get; }
        public int FinishOrder { get; }
        public float FinishTime { get; }
        public IBoostState BoostState { get; }
        public IBoostLedger Ledger { get; }
    }
}
