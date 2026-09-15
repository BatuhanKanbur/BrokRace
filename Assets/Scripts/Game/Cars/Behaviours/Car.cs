using Core.DI.Attributes;
using Core.DI.Inheritances;
using Game.Boost.Interfaces;
using Game.Boost.Logics;
using Game.Boost.Managers;
using Game.Cars.Interfaces;
using Game.Cars.Managers;
using Game.Cars.Structure;
using Game.Configuration.Structure;
using Game.Effects.Behaviours;
using Game.Effects.Interfaces;
using Game.Effects.Managers;
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
        [SerializeField] private CarEffectRig effectRig;
        [SerializeField] private float wheelRadius = 0.3f;

        [Inject] private ITrackPath _track;

        private readonly BoostController _boost = new();
        private ICarVisual _visual;
        private ICarEffects _effects;
        private ICarMotion _motion;
        private BoostSettings _settings;
        private Color _paint;
        private float _laneOffset;
        private float _finishDistance;
        private float _launchSpeedFactor = 1f;
        private float _launchOffset;

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
            _effects = new CarEffects(effectRig);
            _motion = new CarMotion(_boost, _boost);
            _boost.OnBoostStarted += PlayBoostCue;
            _boost.OnBoostEnded += StopBoostCue;
        }

        public void Initialize(CarSetup setup)
        {
            Index = setup.Index;
            DisplayName = setup.DisplayName;
            IsPlayer = setup.IsPlayer;
            _paint = setup.Paint;
            _laneOffset = setup.LaneOffset;
            _finishDistance = setup.FinishDistance;
            _settings = setup.Boost;
            HasFinished = false;
            FinishOrder = 0;
            FinishTime = 0f;

            _boost.Configure(setup.Boost);
            _motion.Configure(setup.NaturalSpeed, setup.FinishDistance);
            _launchSpeedFactor = 1f;
            _launchOffset = 0f;
            _visual.Reset();
            _visual.Paint(setup.Paint);
            _effects.Reset();
            transform.position = _track.GetPosition(0f, _laneOffset);
            transform.rotation = _track.GetRotation(0f);
        }

        public MotionStep Step(float stepTime) => _motion.Advance(stepTime);

        public void SetLaunch(float speedFactor, float distanceOffset)
        {
            _launchSpeedFactor = speedFactor;
            _launchOffset = distanceOffset;
        }

        public void Present(float deltaTime)
        {
            var placement = _motion.Distance + _launchOffset;
            transform.position = _track.GetPosition(placement, _laneOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, _track.GetRotation(placement),
                RotationBlendSpeed * deltaTime);
            var speed = _motion.Speed * _launchSpeedFactor;
            _visual.Present(speed, _motion.Intensity, deltaTime);
            _effects.Present(speed, _motion.Intensity, deltaTime);
        }

        public void SetBalanceScale(float scale) => _motion.SetBalanceScale(scale);

        public void OpenBoostWindow() => _boost.Open();

        public void CloseBoostWindow() => _boost.Close();

        public void MarkFinished(int order, float time)
        {
            HasFinished = true;
            FinishOrder = order;
            FinishTime = time;
            _effects.Celebrate(_paint);
        }

        public void Release() => gameObject.SetActive(false);

        private void PlayBoostCue(int level)
        {
            var color = BoostPalette.ForLevel(_settings, level);
            _visual.PlayBoost(level, color);
            _effects.Play(level, color);
        }

        private void StopBoostCue()
        {
            _visual.StopBoost();
            _effects.Stop();
        }
    }
}
