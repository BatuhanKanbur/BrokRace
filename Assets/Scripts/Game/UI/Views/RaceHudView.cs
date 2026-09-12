using Core.UI.Abstracts;
using Core.Utilities;
using Cysharp.Threading.Tasks;
using Game.Boost.Enums;
using Game.Boost.Interfaces;
using Game.Boost.Logics;
using Game.Configuration.Structure;
using UnityEngine;
using UnityEngine.UI;
using static Game.Boost.Constants.BoostConstants;
using static Game.UI.Constants.HudConstants;

namespace Game.UI.Views
{
    public class RaceHudView : BaseView
    {
        [SerializeField] private Text rankText;
        [SerializeField] private Text speedText;
        [SerializeField] private Text remainingText;
        [SerializeField] private Text boostText;
        [SerializeField] private Text energyText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Image energyFill;
        [SerializeField] private Image boostFill;
        [SerializeField] private Image[] keySlots;
        [SerializeField] private CanvasGroup feedbackGroup;
        [SerializeField] private RectTransform rankCard;

        private BoostSettings _settings;
        private int _shownRank;
        private int _shownSpeed;
        private int _shownRemaining;
        private int _shownEnergy;
        private int _shownLevel;

        public void Bind(BoostSettings settings)
        {
            _settings = settings;
            _shownRank = 0;
            _shownSpeed = -1;
            _shownRemaining = -1;
            _shownEnergy = -1;
            _shownLevel = -1;
        }

        public void Refresh(IBoostState boost, int rank, int total, float speed, float travelled, float raceDistance)
        {
            progressFill.fillAmount = Mathf.Clamp01(travelled / raceDistance);
            energyFill.fillAmount = boost.EnergyRatio;
            boostFill.fillAmount = boost.IsActive ? boost.RemainingWindow / boost.WindowDuration : 0f;

            if (rank != _shownRank)
            {
                var gained = rank < _shownRank && _shownRank > 0;
                _shownRank = rank;
                rankText.text = string.Format(RankFormat, rank, total);
                if (gained) PulseRank().Forget();
            }
            var roundedSpeed = Mathf.RoundToInt(speed);
            if (roundedSpeed != _shownSpeed)
            {
                _shownSpeed = roundedSpeed;
                speedText.text = string.Format(SpeedFormat, roundedSpeed);
            }
            var remaining = Mathf.RoundToInt(Mathf.Max(0f, raceDistance - travelled));
            if (remaining != _shownRemaining)
            {
                _shownRemaining = remaining;
                remainingText.text = string.Format(RemainingFormat, remaining);
            }
            var energy = Mathf.FloorToInt(boost.Energy);
            if (energy != _shownEnergy)
            {
                _shownEnergy = energy;
                energyText.text = energy.ToString();
            }
            if (boost.IsActive)
                boostText.text = string.Format(ActiveBoostFormat, boost.ActiveLevel, boost.RemainingWindow);
            else if (_shownLevel != 0)
                boostText.text = ReadyLabel;
            if (boost.ActiveLevel != _shownLevel)
            {
                _shownLevel = boost.ActiveLevel;
                boostFill.color = BoostPalette.ForLevel(_settings, Mathf.Max(_shownLevel, MinLevel));
            }
            RefreshKeys(boost);
        }

        public void FlashAccepted(int level)
        {
            feedbackText.text = string.Format(AcceptedFormat, level);
            feedbackText.color = BoostPalette.ForLevel(_settings, level);
            Pulse();
        }

        public void FlashRejected(BoostRequestOutcome outcome)
        {
            feedbackText.text = BoostFeedback.Describe(outcome);
            feedbackText.color = RejectColor;
            Pulse();
        }

        private void RefreshKeys(IBoostState boost)
        {
            for (var index = 0; index < keySlots.Length; index++)
            {
                var level = index + MinLevel;
                keySlots[index].color = boost.ActiveLevel == level
                    ? BoostPalette.ForLevel(_settings, level)
                    : IsBlocked(boost, level) ? BlockedColor : ReadyColor;
            }
        }

        private static bool IsBlocked(IBoostState boost, int level) =>
            boost.IsActive || boost.RemainingCooldown > 0f || !boost.CanAfford(level);

        private async UniTaskVoid PulseRank()
        {
            var token = this.GetCancellationTokenOnDestroy();
            var elapsed = 0f;
            while (elapsed < RankPulseDuration)
            {
                elapsed += Time.deltaTime;
                var wave = Mathf.Sin(elapsed / RankPulseDuration * Mathf.PI);
                rankCard.localScale = Vector3.one * Mathf.LerpUnclamped(1f, RankPulseScale, wave);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            rankCard.localScale = Vector3.one;
        }

        private void Pulse()
        {
            feedbackGroup.alpha = 1f;
            feedbackGroup.GoFade(0f, FeedbackFadeDuration, token: this.GetCancellationTokenOnDestroy());
        }
    }
}
