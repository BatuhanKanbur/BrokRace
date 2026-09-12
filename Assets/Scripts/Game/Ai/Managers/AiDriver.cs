using Core.Utilities;
using Game.Ai.Interfaces;
using Game.Ai.Structure;
using Game.Boost.Logics;
using Game.Cars.Interfaces;
using Game.Configuration.Structure;
using Game.Race.Interfaces;
using Game.Standings.Interfaces;
using UnityEngine;
using static Game.Ai.Constants.AiConstants;
using static Game.Boost.Constants.BoostConstants;
using static Game.Race.Constants.RaceConstants;

namespace Game.Ai.Managers
{
    public class AiDriver : IAiDriver
    {
        private readonly ICar _car;
        private readonly AiProfile _profile;
        private readonly AiSettings _settings;
        private readonly BoostSettings _economy;
        private readonly IRaceState _race;
        private readonly IRaceOrder _order;
        private readonly float _stepTime;
        private readonly int _seed;
        private readonly float _targetPace;

        private int _stepsLeft;
        private int _decisionIndex;

        public int CarIndex => _car.Index;
        public string ProfileName => _profile.name;
        public AiDecision LastDecision { get; private set; }

        public AiDriver(ICar car, AiProfile profile, AiSettings settings, BoostSettings economy,
            IRaceState race, IRaceOrder order, float stepTime, int seed)
        {
            _car = car;
            _profile = profile;
            _settings = settings;
            _economy = economy;
            _race = race;
            _order = order;
            _stepTime = stepTime;
            _seed = seed;
            _targetPace = race.RaceDistance / profile.targetFinishTime;
            Reset();
        }

        public int Decide()
        {
            if (--_stepsLeft > 0) return NoDecision;
            _stepsLeft = IntervalSteps(Jitter());
            if (_car.BoostState.IsActive || _car.BoostState.RemainingCooldown > 0f) return NoDecision;
            return Choose();
        }

        public void Reset()
        {
            _decisionIndex = 0;
            _stepsLeft = IntervalSteps(_profile.decisionPhase);
            LastDecision = default;
        }

        private int IntervalSteps(float offset) =>
            Mathf.Max(1, Mathf.RoundToInt(_profile.decisionInterval * (1f + offset) / _stepTime));

        private float Jitter() => _profile.decisionJitter * NoiseAt(_decisionIndex + NoiseSalt);

        private float NoiseAt(int index) =>
            DeterministicRandom.Stream(_seed, _car.Index * NoiseSalt + index).Signed();

        private int Choose()
        {
            var strike = 0f;
            var defend = DefendPressure();
            var pace = PaceDeficit();
            var closing = ClosingUrge();
            var spill = SpillRisk();
            var noise = NoiseAt(_decisionIndex);
            _decisionIndex++;

            var best = NoDecision;
            var bestScore = _profile.holdBias + _settings.holdSaveWeight * (1f - spill) * (1f - closing);
            var holdScore = bestScore;
            var bestStrike = 0f;

            var reserve = _profile.reserveEnergy * (1f - closing);
            if (_car.Progress >= _settings.burnDownProgress) reserve = 0f;

            for (var level = _settings.minLevel; level <= MaxLevel; level++)
            {
                if (level > _profile.levelBias + 1 && _car.Progress < _settings.burnDownProgress) continue;
                var cost = _car.BoostState.CostOf(level);
                if (!_car.BoostState.CanAfford(level)) continue;
                if (_car.BoostState.Energy - cost < reserve) continue;
                strike = StrikePressure(level);
                var score = _profile.strikeWeight * strike
                            + _profile.defendWeight * defend
                            + _profile.paceWeight * pace
                            + _profile.closeWeight * closing * (level / (float)MaxLevel)
                            + _profile.spillWeight * spill
                            + _profile.efficiencyWeight * BoostEconomy.Efficiency(_economy, _car.NaturalSpeed, level, MinLevel + 1)
                            + _settings.levelFitWeight * (1f - Mathf.Abs(level - _profile.levelBias) / (float)LevelSpan)
                            + _profile.noiseWeight * noise
                            - _settings.costWeight * (cost / _car.BoostState.Capacity);
                if (score <= bestScore) continue;
                bestScore = score;
                best = level;
                bestStrike = strike;
            }

            LastDecision = new AiDecision(best, bestStrike, defend, pace, closing, spill, bestScore, holdScore);
            return best;
        }

        private float StrikePressure(int level)
        {
            var ahead = _order.CarAhead(_car);
            if (ahead == null) return 0f;
            var gap = ahead.Distance - _car.Distance;
            var reach = (level - NeutralLevel) * _car.NaturalSpeed * _economy.windowDuration;
            return Mathf.Max(0f, 1f - Mathf.Abs(gap - reach) / _settings.strikeTolerance);
        }

        private float DefendPressure()
        {
            var behind = _order.CarBehind(_car);
            var gap = behind != null ? _car.Distance - behind.Distance : _settings.noNeighbourGap;
            return Mathf.Max(0f, 1f - gap / _settings.defendRange);
        }

        private float PaceDeficit() =>
            Mathf.Clamp01((_targetPace * _race.Time - _car.Distance) / _settings.paceDeficitSpan);

        private float ClosingUrge() =>
            Mathf.Clamp01((_car.Progress - _profile.closeFrom) / (1f - _profile.closeFrom));

        private float SpillRisk()
        {
            var threshold = _settings.spillThreshold;
            return Mathf.Clamp01((_car.BoostState.EnergyRatio - threshold) / (1f - threshold));
        }
    }
}
