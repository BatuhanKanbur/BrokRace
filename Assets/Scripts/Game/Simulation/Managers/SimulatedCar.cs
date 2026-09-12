using Game.Boost.Interfaces;
using Game.Boost.Managers;
using Game.Cars.Interfaces;
using Game.Cars.Managers;
using Game.Cars.Structure;

namespace Game.Simulation.Managers
{
    public class SimulatedCar : ICar
    {
        private readonly BoostController _boost = new();
        private readonly CarMotion _motion;

        private float _finishDistance;

        public SimulatedCar() => _motion = new CarMotion(_boost, _boost);

        public int Index { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsPlayer { get; private set; }
        public float Distance => _motion.Distance;
        public float Speed => _motion.Speed;
        public float NaturalSpeed => _motion.NaturalSpeed;
        public float BalanceScale => _motion.BalanceScale;
        public float BoostDistance => _motion.BoostDistance;
        public float AssistDistance => _motion.AssistDistance;
        public float Progress => _motion.Distance / _finishDistance;
        public bool HasFinished { get; private set; }
        public int FinishOrder { get; private set; }
        public float FinishTime { get; private set; }
        public IBoostController Boost => _boost;
        public IBoostState BoostState => _boost;
        public IBoostLedger Ledger => _boost;

        public void Initialize(CarSetup setup)
        {
            Index = setup.Index;
            DisplayName = setup.DisplayName;
            IsPlayer = setup.IsPlayer;
            _finishDistance = setup.FinishDistance;
            HasFinished = false;
            FinishOrder = 0;
            FinishTime = 0f;
            _boost.Configure(setup.Boost);
            _motion.Configure(setup.NaturalSpeed, setup.FinishDistance);
        }

        public MotionStep Step(float stepTime) => _motion.Advance(stepTime);

        public void SetBalanceScale(float scale) => _motion.SetBalanceScale(scale);

        public void OpenBoostWindow() => _boost.Open();

        public void CloseBoostWindow() => _boost.Close();

        public void MarkFinished(int order, float time)
        {
            HasFinished = true;
            FinishOrder = order;
            FinishTime = time;
        }
    }
}
