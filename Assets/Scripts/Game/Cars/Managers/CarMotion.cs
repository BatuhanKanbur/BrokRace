using Game.Boost.Interfaces;
using Game.Boost.Logics;
using Game.Cars.Interfaces;
using Game.Cars.Structure;
using UnityEngine;
using static Game.Boost.Constants.BoostConstants;
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
        public float Intensity => BaseSpeed > 0f ? (Speed / BaseSpeed - 1f) / LevelSpan : 0f;

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
                travelled += Integrate(boosted, _boost.Multiplier - 1f,
                    _boost.WindowDuration - _boost.RemainingWindow, 0f, ref crossOffset);
            var plain = stepTime - boosted;
            if (plain > 0f)
                travelled += Integrate(plain, 0f, 0f, boosted, ref crossOffset);
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

        private float Integrate(float slice, float gain, float windowStart, float sliceStart, ref float crossOffset)
        {
            var effective = Effective(slice, gain, windowStart);
            var span = BaseSpeed * effective;
            if (crossOffset < 0f && Distance < _finishDistance && Distance + span >= _finishDistance)
                crossOffset = sliceStart + SolveSlice((_finishDistance - Distance) / BaseSpeed, gain, windowStart, slice);
            BaseDistance += NaturalSpeed * slice;
            BoostDistance += NaturalSpeed * (effective - slice);
            AssistDistance += NaturalSpeed * (BalanceScale - 1f) * effective;
            Distance += span;
            Speed = BaseSpeed * (1f + gain * RateAt(windowStart + slice));
            _clock.Advance(slice);
            return span;
        }

        private float Effective(float slice, float gain, float windowStart) =>
            gain > 0f
                ? slice + gain * _boost.WindowDuration * (IntegralAt(windowStart + slice) - IntegralAt(windowStart))
                : slice;

        private float SolveSlice(float target, float gain, float windowStart, float slice)
        {
            if (gain <= 0f) return Mathf.Clamp(target, 0f, slice);
            var guess = target / (1f + gain * RateAt(windowStart));
            for (var pass = 0; pass < CrossingRefinements; pass++)
            {
                var reached = Effective(guess, gain, windowStart);
                var slope = 1f + gain * RateAt(windowStart + guess);
                guess -= (reached - target) / slope;
            }
            return Mathf.Clamp(guess, 0f, slice);
        }

        private float IntegralAt(float windowTime) =>
            BoostCurves.Integral(_boost.Curve, windowTime / _boost.WindowDuration, _boost.Settings);

        private float RateAt(float windowTime) =>
            BoostCurves.Rate(_boost.Curve, windowTime / _boost.WindowDuration, _boost.Settings);
    }
}
