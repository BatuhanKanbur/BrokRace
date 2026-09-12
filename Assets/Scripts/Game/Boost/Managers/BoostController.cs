using System;
using Game.Boost.Enums;
using Game.Boost.Logics;
using Game.Boost.Interfaces;
using Game.Configuration.Structure;
using UnityEngine;
using static Game.Boost.Constants.BoostConstants;

namespace Game.Boost.Managers
{
    public class BoostController : IBoostController, IBoostState, IBoostClock, IBoostLedger
    {
        private BoostSettings _settings;
        private int _activeLevel;
        private float _remainingWindow;
        private float _remainingCooldown;
        private float _energy;
        private bool _isOpen;

        public event Action<int> OnBoostStarted;
        public event Action OnBoostEnded;
        public event Action<int, BoostRequestOutcome> OnRequestRejected;

        public bool IsActive => _activeLevel > 0;
        public int ActiveLevel => _activeLevel;
        public float Multiplier => _activeLevel > 0 ? _activeLevel : NeutralLevel;
        public BoostCurve Curve => CurveOf(_activeLevel > 0 ? _activeLevel : MinLevel);
        public BoostSettings Settings => _settings;
        public float WindowDuration => _settings.windowDuration;
        public float RemainingWindow => _remainingWindow;
        public float RemainingCooldown => _remainingCooldown;
        public float Energy => _energy;
        public float Capacity => _settings.energyCapacity;
        public float EnergyRatio => _energy / _settings.energyCapacity;
        public int AcceptedCount { get; private set; }
        public int RejectedCount { get; private set; }
        public int ExtraLevelSum { get; private set; }
        public float EnergySpent { get; private set; }
        public float EnergyWasted { get; private set; }
        public float BoostedSeconds { get; private set; }

        public void Configure(BoostSettings settings)
        {
            _settings = settings;
            Reset();
        }

        public float CostOf(int level) => _settings.levelCosts[Clamp(level) - MinLevel];

        public BoostCurve CurveOf(int level) => _settings.levelCurves[Clamp(level) - MinLevel];

        public bool CanAfford(int level) => _energy >= CostOf(level);

        public void Open() => _isOpen = true;

        public void Close()
        {
            _isOpen = false;
            EndWindow();
        }

        public BoostRequestOutcome Request(int requestedLevel)
        {
            var level = Clamp(requestedLevel);
            var outcome = Evaluate(level);
            if (outcome != BoostRequestOutcome.Accepted)
            {
                RejectedCount++;
                OnRequestRejected?.Invoke(level, outcome);
                return outcome;
            }
            var cost = CostOf(level);
            _energy -= cost;
            EnergySpent += cost;
            AcceptedCount++;
            ExtraLevelSum += level - NeutralLevel;
            _activeLevel = level;
            _remainingWindow = _settings.windowDuration;
            OnBoostStarted?.Invoke(level);
            return outcome;
        }

        public void Advance(float slice)
        {
            if (IsActive)
            {
                _remainingWindow -= slice;
                BoostedSeconds += slice;
                if (_remainingWindow > WindowEpsilon) return;
                EndWindow();
                _remainingCooldown = _settings.cooldown;
                return;
            }
            if (_remainingCooldown > 0f)
                _remainingCooldown = Mathf.Max(0f, _remainingCooldown - slice);
        }

        public void Tick(float stepTime)
        {
            if (!_isOpen) return;
            var gained = _settings.energyRegenPerSecond * stepTime;
            var headroom = _settings.energyCapacity - _energy;
            if (gained > headroom) EnergyWasted += gained - headroom;
            _energy = Mathf.Min(_settings.energyCapacity, _energy + gained);
        }

        public void Reset()
        {
            _activeLevel = 0;
            _remainingWindow = 0f;
            _remainingCooldown = 0f;
            _energy = _settings.energyOnStart;
            _isOpen = false;
            AcceptedCount = 0;
            RejectedCount = 0;
            ExtraLevelSum = 0;
            EnergySpent = 0f;
            EnergyWasted = 0f;
            BoostedSeconds = 0f;
        }

        private static int Clamp(int level) => Mathf.Clamp(level, MinLevel, MaxLevel);

        private BoostRequestOutcome Evaluate(int level)
        {
            if (!_isOpen) return BoostRequestOutcome.NotRunning;
            if (IsActive) return BoostRequestOutcome.WindowActive;
            if (_remainingCooldown > 0f) return BoostRequestOutcome.OnCooldown;
            return CanAfford(level) ? BoostRequestOutcome.Accepted : BoostRequestOutcome.InsufficientEnergy;
        }

        private void EndWindow()
        {
            if (_activeLevel == 0) return;
            _activeLevel = 0;
            _remainingWindow = 0f;
            OnBoostEnded?.Invoke();
        }
    }
}
