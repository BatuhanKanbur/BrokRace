using Core.DI.Attributes;
using Core.DI.Inheritances;
using Game.Boost.Interfaces;
using Game.Boost.Logics;
using Game.Boost.Managers;
using Game.Cars.Interfaces;
using Game.Cars.Managers;
using Game.Cars.Structure;
using Game.Configuration.Structure;
using Game.Track.Interfaces;
using UnityEngine;
using static Game.Cars.Constants.CarConstants;

namespace Game.Cars.Behaviours
{
    public class Car : BaseMonoBehaviour, ICar, ICarView
    {
        [SerializeField] private PaintSlot[] paintSlots;
        [SerializeField] private Transform[] wheels;
        [SerializeField] private Transform body;
        [SerializeField] private TrailRenderer[] trails;
        [SerializeField] private float wheelRadius = 0.3f;

        [Inject] private ITrackPath _track;

        private readonly BoostController _boost = new();
        private ICarVisual _visual;
        private ICarMotion _motion;
        private BoostSettings _settings;
        private float _laneOffset;
        private float _finishDistance;

        public Transform Transform => transform;
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

        protected override void Awake()
        {
            base.Awake();
            _visual = new CarVisual(paintSlots, wheels, body, trails, wheelRadius);
            _motion = new CarMotion(_boost, _boost);
            _boost.OnBoostStarted += PlayBoostCue;
            _boost.OnBoostEnded += _visual.StopBoost;
        }

        public void Initialize(CarSetup setup)
        {
            Index = setup.Index;
            DisplayName = setup.DisplayName;
            IsPlayer = setup.IsPlayer;
            _laneOffset = setup.LaneOffset;
            _finishDistance = setup.FinishDistance;
            _settings = setup.Boost;
            HasFinished = false;
            FinishOrder = 0;
            FinishTime = 0f;

            _boost.Configure(setup.Boost);
            _motion.Configure(setup.NaturalSpeed, setup.FinishDistance);
            _visual.Reset();
            _visual.Paint(setup.Paint);
            transform.position = _track.GetPosition(0f, _laneOffset);
            transform.rotation = _track.GetRotation(0f);
        }

        public MotionStep Step(float stepTime) => _motion.Advance(stepTime);

        public void Present(float deltaTime)
        {
            transform.position = _track.GetPosition(_motion.Distance, _laneOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, _track.GetRotation(_motion.Distance),
                RotationBlendSpeed * deltaTime);
            _visual.Present(_motion.Speed, deltaTime);
        }

        public void SetBalanceScale(float scale) => _motion.SetBalanceScale(scale);

        public void OpenBoostWindow() => _boost.Open();

        public void CloseBoostWindow() => _boost.Close();

        public void MarkFinished(int order, float time)
        {
            HasFinished = true;
            FinishOrder = order;
            FinishTime = time;
        }

        public void Release() => gameObject.SetActive(false);

        private void PlayBoostCue(int level) => _visual.PlayBoost(level, BoostPalette.ForLevel(_settings, level));
    }
}
