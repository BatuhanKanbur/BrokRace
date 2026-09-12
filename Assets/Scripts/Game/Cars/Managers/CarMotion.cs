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
        private readonly float _finishDistance;
        private readonly float _naturalSpeed;

        public float Distance { get; private set; }
        public float Speed { get; private set; }
        public float BalanceScale { get; private set; } = 1f;
        public float BoostDistance { get; private set; }
        public float BaseSpeed => _naturalSpeed * BalanceScale;

        public CarMotion(IBoostState boost, IBoostClock clock, float naturalSpeed, float finishDistance)
        {
            _boost = boost;
            _clock = clock;
            _naturalSpeed = naturalSpeed;
            _finishDistance = finishDistance;
            Speed = naturalSpeed;
        }

        public MotionStep Advance(float stepTime)
        {
            var crossOffset = NoCrossing;
            var travelled = 0f;
            var boosted = _boost.IsActive ? Mathf.Min(stepTime, _boost.RemainingWindow) : 0f;
            if (boosted > 0f)
            {
                var multiplier = _boost.Multiplier;
                travelled += Integrate(BaseSpeed * multiplier, boosted, 0f, ref crossOffset);
                BoostDistance += BaseSpeed * (multiplier - 1f) * boosted;
            }
            var plain = stepTime - boosted;
            if (plain > 0f)
                travelled += Integrate(BaseSpeed, plain, boosted, ref crossOffset);
            _clock.Tick(stepTime);
            return new MotionStep(travelled, crossOffset);
        }

        public void SetBalanceScale(float scale) => BalanceScale = scale;

        public void Reset()
        {
            Distance = 0f;
            Speed = _naturalSpeed;
            BalanceScale = 1f;
            BoostDistance = 0f;
        }

        private float Integrate(float speed, float slice, float sliceStart, ref float crossOffset)
        {
            var step = speed * slice;
            if (crossOffset < 0f && Distance < _finishDistance && Distance + step >= _finishDistance)
                crossOffset = sliceStart + (_finishDistance - Distance) / speed;
            Distance += step;
            Speed = speed;
            _clock.Advance(slice);
            return step;
        }
    }
}
