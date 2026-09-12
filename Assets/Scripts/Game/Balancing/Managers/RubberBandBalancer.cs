using System.Collections.Generic;
using Game.Balancing.Interfaces;
using Game.Cars.Interfaces;
using Game.Configuration.Structure;
using Game.Race.Interfaces;
using UnityEngine;

namespace Game.Balancing.Managers
{
    public class RubberBandBalancer : IRaceBalancer
    {
        private readonly BalanceSettings _settings;
        private readonly IRaceState _race;
        private readonly Dictionary<int, float> _scales = new();

        public bool IsEnabled => _settings.enabled;

        public RubberBandBalancer(BalanceSettings settings, IRaceState race)
        {
            _settings = settings;
            _race = race;
        }

        public float ScaleFor(ICarProgress car) => _scales[car.Index];

        public void Step(float stepTime)
        {
            if (!_settings.enabled) return;
            var player = _race.Player;
            var fade = Fade(player.Distance / _race.RaceDistance);
            foreach (var car in _race.Cars)
            {
                if (car.IsPlayer) continue;
                var target = Target(car.Distance - player.Distance, fade);
                var scale = Mathf.MoveTowards(_scales[car.Index], target, _settings.changeRatePerSecond * stepTime);
                _scales[car.Index] = scale;
                car.SetBalanceScale(scale);
            }
        }

        public void Reset()
        {
            _scales.Clear();
            foreach (var car in _race.Cars)
            {
                _scales[car.Index] = 1f;
                car.SetBalanceScale(1f);
            }
        }

        private float Target(float gap, float fade)
        {
            if (gap < -_settings.deadZone)
            {
                var pull = Mathf.InverseLerp(_settings.deadZone, _settings.fullEffectGap, -gap);
                return Mathf.Lerp(1f, _settings.maxCatchUpScale, pull * fade);
            }
            if (gap > _settings.leaderRunawayGap)
            {
                var hold = Mathf.InverseLerp(_settings.leaderRunawayGap,
                    _settings.leaderRunawayGap + _settings.fullEffectGap, gap);
                return Mathf.Lerp(1f, _settings.minLeaderScale, hold * fade);
            }
            return 1f;
        }

        private float Fade(float progress) => progress <= _settings.fadeStartProgress
            ? 1f
            : 1f - Mathf.InverseLerp(_settings.fadeStartProgress, 1f, progress);
    }
}
