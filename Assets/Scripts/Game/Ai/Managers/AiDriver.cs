using Core.Utilities;
using Game.Ai.Interfaces;
using Game.Boost.Enums;
using Game.Cars.Interfaces;
using Game.Configuration.Structure;
using Game.Race.Interfaces;
using Game.Standings.Interfaces;
using UnityEngine;
using static Game.Ai.Constants.AiConstants;
using static Game.Boost.Constants.BoostConstants;

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
        private readonly DeterministicRandom _random;

        private float _countdown;
        private string _stateLabel = IdleLabel;

        public int CarIndex => _car.Index;
        public string ProfileName => _profile.name;
        public string StateLabel => _stateLabel;

        public AiDriver(ICar car, AiProfile profile, AiSettings settings, BoostSettings economy,
            IRaceState race, IRaceOrder order, DeterministicRandom random)
        {
            _car = car;
            _profile = profile;
            _settings = settings;
            _economy = economy;
            _race = race;
            _order = order;
            _random = random;
            Reset();
        }

        public void Step(float stepTime)
        {
            _countdown -= stepTime;
            if (_countdown > 0f) return;
            _countdown = NextInterval();
            Decide();
        }

        public void Reset()
        {
            _countdown = _profile.decisionPhase * _profile.decisionInterval + NextInterval();
            _stateLabel = IdleLabel;
        }

        private float NextInterval() =>
            _profile.decisionInterval * (1f + _profile.decisionJitter * _random.Signed());

        private void Decide()
        {
            var chase = ChasePressure();
            var defend = DefendPressure();
            var phase = PhaseUrge(_car.Distance / _race.RaceDistance);
            var reserve = ReserveUrge();
            var appetite = _profile.aggression
                           + _profile.chaseWeight * chase
                           + _profile.defendWeight * defend
                           + _profile.attackWindowWeight * phase
                           + _profile.noise * _random.Signed();

            var bestLevel = 0;
            var bestScore = _settings.actionThreshold;
            for (var level = NeutralLevel + 1; level <= Mathf.Min(_profile.levelCap, MaxLevel); level++)
            {
                if (!_car.BoostState.CanAfford(level)) continue;
                var gain = level - NeutralLevel;
                var cost = _car.BoostState.CostOf(level) / _economy.energyCapacity;
                var score = appetite * gain - _profile.patience * reserve * cost * _settings.costWeight;
                if (score <= bestScore) continue;
                bestScore = score;
                bestLevel = level;
            }

            if (bestLevel == 0)
            {
                _stateLabel = $"{HoldLabel} c{chase:0.00} d{defend:0.00} p{phase:0.00} e{_car.BoostState.EnergyRatio:0.00}";
                return;
            }
            var outcome = _car.Boost.Request(bestLevel);
            _stateLabel = outcome == BoostRequestOutcome.Accepted
                ? $"x{bestLevel} c{chase:0.00} d{defend:0.00} p{phase:0.00}"
                : $"{outcome} c{chase:0.00} d{defend:0.00} p{phase:0.00}";
        }

        private float ChasePressure()
        {
            var ahead = _order.CarAhead(_car);
            if (ahead == null) return 0f;
            var gap = ahead.Distance - _car.Distance;
            if (gap > _profile.strikeRange) return 0f;
            var pressure = 1f - gap / _profile.strikeRange;
            return ahead.IsPlayer ? pressure * (1f + _profile.playerFocus) : pressure;
        }

        private float DefendPressure()
        {
            var behind = _order.CarBehind(_car);
            if (behind == null) return 0f;
            var gap = _car.Distance - behind.Distance;
            if (gap > _profile.strikeRange) return 0f;
            var pressure = 1f - gap / _profile.strikeRange;
            return behind.IsPlayer ? pressure * (1f + _profile.playerFocus) : pressure;
        }

        private float PhaseUrge(float progress)
        {
            if (progress >= _profile.attackWindow.x && progress <= _profile.attackWindow.y) return 1f;
            var outside = progress < _profile.attackWindow.x
                ? _profile.attackWindow.x - progress
                : progress - _profile.attackWindow.y;
            return Mathf.Max(0f, 1f - outside / PhaseFalloff);
        }

        private float ReserveUrge()
        {
            var scarcity = Mathf.InverseLerp(1f, _profile.energyFloor, _car.BoostState.EnergyRatio);
            return BaseReserveUrge + _settings.reserveUrgency * scarcity * scarcity;
        }
    }
}
