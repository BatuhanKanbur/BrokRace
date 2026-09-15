using System.Collections.Generic;
using Game.Balancing.Interfaces;
using Game.Cars.Interfaces;
using Game.Configuration.Structure;
using Game.Race.Interfaces;
using Game.Standings.Interfaces;
using UnityEngine;

namespace Game.Balancing.Managers
{
    public class RubberBandBalancer : IRaceBalancer
    {
        private readonly BalanceSettings _settings;
        private readonly IRaceState _race;
        private readonly IRaceOrder _order;
        private readonly Dictionary<int, float> _scales = new();
        private readonly Dictionary<int, float> _responses = new();

        public bool IsEnabled { get; private set; }
        public bool IsSuspended { get; private set; }

        public RubberBandBalancer(BalanceSettings settings, IRaceState race, IRaceOrder order)
        {
            _settings = settings;
            _race = race;
            _order = order;
            IsEnabled = settings.enabled;
        }

        public void Enable(bool enabled) => IsEnabled = enabled;

        public float ScaleFor(ICarProgress car) => _scales[car.Index];

        public void Register(ICarProgress car, float response) => _responses[car.Index] = response;

        public void Step(float stepTime)
        {
            if (!IsEnabled) return;
            var player = _race.Player;
            IsSuspended = IsPlayerCoasting(player);
            var leader = _order.Leader;
            var runaway = RunawayGap(leader);
            foreach (var car in _race.Cars)
            {
                if (car.IsPlayer || car.BoostState.IsActive) continue;
                var target = IsSuspended ? 1f : Target(car, player, leader, runaway);
                var scale = Mathf.MoveTowards(_scales[car.Index], target, _settings.changeRatePerSecond * stepTime);
                _scales[car.Index] = scale;
                car.SetBalanceScale(scale);
            }
        }

        public void Reset()
        {
            IsSuspended = false;
            _scales.Clear();
            foreach (var car in _race.Cars)
            {
                _scales[car.Index] = 1f;
                car.SetBalanceScale(1f);
            }
        }

        private float Target(ICarProgress car, ICarProgress player, ICarProgress leader, float runaway)
        {
            var fade = Fade(car.Progress) * Response(car);
            if (fade <= 0f) return 1f;
            var visibilityGap = player.Distance - car.Distance;
            if (visibilityGap > _settings.muteBand
                && _order.RankOf(car) > _order.RankOf(player)
                && car.AssistDistance < _settings.metreBudget)
            {
                var pull = Mathf.InverseLerp(_settings.muteBand, _settings.fullAssistGap, visibilityGap);
                return Mathf.Lerp(1f, _settings.maxAssistScale, pull * fade);
            }
            if (!leader.IsPlayer && car.Index == leader.Index
                && runaway > _settings.runawayGap
                && car.AssistDistance > -_settings.metreBudget)
            {
                var hold = Mathf.InverseLerp(_settings.runawayGap, _settings.fullRestraintGap, runaway);
                return Mathf.Lerp(1f, _settings.minRestraintScale, hold * fade);
            }
            return 1f;
        }

        private float RunawayGap(ICarProgress leader)
        {
            var second = _order.Order.Count > 1 ? _order.Order[1] : leader;
            return leader.Distance - second.Distance;
        }

        private float Response(ICarProgress car) => _responses[car.Index];

        private float Fade(float progress)
        {
            if (progress <= _settings.fadeStartProgress) return 1f;
            if (progress >= _settings.fadeEndProgress) return 0f;
            return 1f - Mathf.InverseLerp(_settings.fadeStartProgress, _settings.fadeEndProgress, progress);
        }

        private bool IsPlayerCoasting(ICarProgress player)
        {
            if (_order.RankOf(player) < _race.Cars.Count) return false;
            var available = player.BoostState.Capacity + player.Ledger.EnergySpent;
            return player.Ledger.EnergySpent < available * _settings.passiveSpendThreshold;
        }
    }
}
