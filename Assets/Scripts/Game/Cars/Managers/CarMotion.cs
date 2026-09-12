using Game.Boost.Interfaces;
using Game.Cars.Interfaces;
using Game.Cars.Structure;
using UnityEngine;
using static Game.Cars.Constants.CarConstants;

namespace Game.Cars.Managers
{
    public class CarMotion : ICarMotion
    {
        private readonly IBoostState _boost;
        private readonly IBoostClock _clock;

        private float _finishDistance;

        public float Distance { get; private set; }
        public float Speed { get; private set; }
        public float NaturalSpeed { get; private set; }
        public float BalanceScale { get; private set; } = 1f;
        public float BaseDistance { get; private set; }
        public float BoostDistance { get; private set; }
        public float AssistDistance { get; private set; }
        public float BaseSpeed => NaturalSpeed * BalanceScale;

        public CarMotion(IBoostState boost, IBoostClock clock)
        {
            _boost = boost;
            _clock = clock;
        }

        public void Configure(float naturalSpeed, float finishDistance)
        {
            NaturalSpeed = naturalSpeed;
            _finishDistance = finishDistance;
            Reset();
        }

        public MotionStep Advance(float stepTime)
        {
            var crossOffset = NoCrossing;
            var travelled = 0f;
            var boosted = _boost.IsActive ? Mathf.Min(stepTime, _boost.RemainingWindow) : 0f;
            if (boosted > 0f)
                travelled += Integrate(_boost.Multiplier, boosted, 0f, ref crossOffset);
            var plain = stepTime - boosted;
            if (plain > 0f)
                travelled += Integrate(1f, plain, boosted, ref crossOffset);
            _clock.Tick(stepTime);
            return new MotionStep(travelled, crossOffset);
        }

        public void SetBalanceScale(float scale) => BalanceScale = scale;

        public void Reset()
        {
            Distance = 0f;
            Speed = NaturalSpeed;
            BalanceScale = 1f;
            BaseDistance = 0f;
            BoostDistance = 0f;
            AssistDistance = 0f;
        }

        private float Integrate(float multiplier, float slice, float sliceStart, ref float crossOffset)
        {
            var speed = BaseSpeed * multiplier;
            var step = speed * slice;
            if (crossOffset < 0f && Distance < _finishDistance && Distance + step >= _finishDistance)
                crossOffset = sliceStart + (_finishDistance - Distance) / speed;
            BaseDistance += NaturalSpeed * slice;
            BoostDistance += NaturalSpeed * (multiplier - 1f) * slice;
            AssistDistance += NaturalSpeed * (BalanceScale - 1f) * multiplier * slice;
            Distance += step;
            Speed = speed;
            _clock.Advance(slice);
            return step;
        }
    }
}
