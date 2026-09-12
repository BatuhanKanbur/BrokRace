using System;
using Core.DI.Attributes;
using Core.DI.Inheritances;
using Game.Boost.Interfaces;
using Game.Boost.Managers;
using Game.Cars.Interfaces;
using Game.Cars.Managers;
using Game.Cars.Structure;
using Game.Track.Interfaces;
using UnityEngine;
using static Game.Cars.Constants.CarConstants;

namespace Game.Cars.Behaviours
{
    public class Car : BaseMonoBehaviour, ICar
    {
        [SerializeField] private PaintSlot[] paintSlots;
        [SerializeField] private Transform[] wheels;
        [SerializeField] private Transform body;
        [SerializeField] private TrailRenderer[] trails;
        [SerializeField] private float wheelRadius = 0.3f;

        [Inject] private ITrackPath _track;

        private ICarVisual _visual;
        private ICarMotion _motion;
        private BoostController _boost;
        private float _laneOffset;

        public event Action<ICar, float> OnCrossedFinish;

        public Transform Transform => transform;
        public int Index { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsPlayer { get; private set; }
        public float Distance => _motion.Distance;
        public float Speed => _motion.Speed;
        public float BaseSpeed => _motion.BaseSpeed;
        public float BalanceScale => _motion.BalanceScale;
        public float BoostDistance => _motion.BoostDistance;
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
        }

        public void Initialize(CarSetup setup)
        {
            Index = setup.Index;
            DisplayName = setup.DisplayName;
            IsPlayer = setup.IsPlayer;
            _laneOffset = setup.LaneOffset;
            HasFinished = false;
            FinishOrder = 0;
            FinishTime = 0f;

            Detach();
            _boost = new BoostController(setup.Boost);
            _boost.OnBoostStarted += _visual.PlayBoost;
            _boost.OnBoostEnded += _visual.StopBoost;
            _motion = new CarMotion(_boost, _boost, setup.NaturalSpeed, setup.FinishDistance);

            _visual.Reset();
            _visual.Paint(setup.Paint);
            transform.position = _track.GetPosition(0f, _laneOffset);
            transform.rotation = _track.GetRotation(0f);
        }

        public void Step(float stepTime)
        {
            var step = _motion.Advance(stepTime);
            if (step.HasCrossed && !HasFinished)
                OnCrossedFinish?.Invoke(this, step.CrossOffset);
        }

        public void Present(float deltaTime)
        {
            transform.position = _track.GetPosition(_motion.Distance, _laneOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, _track.GetRotation(_motion.Distance),
                RotationBlendSpeed * deltaTime);
            _visual.Tick(_motion.Speed, deltaTime);
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

        public void Release()
        {
            Detach();
            gameObject.SetActive(false);
        }

        private void Detach()
        {
            if (_boost == null) return;
            _boost.OnBoostStarted -= _visual.PlayBoost;
            _boost.OnBoostEnded -= _visual.StopBoost;
        }
    }
}
