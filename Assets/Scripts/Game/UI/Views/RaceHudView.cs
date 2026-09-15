using System;
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
        [SerializeField] private Slider progressBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private Slider boostBar;
        [SerializeField] private Button[] boostButtons;
        [SerializeField] private Button overlayButton;
        [SerializeField] private CanvasGroup feedbackGroup;
        [SerializeField] private RectTransform rankCard;

        private BoostSettings _settings;
        private Image _energyFill;
        private Image _boostFill;
        private Color _energyTint;
        private float _restingDial;
        private float _peakDial;
        private float _dial;
        private float _energyFlash;
        private int _shownRank;
        private int _shownSpeed;
        private int _shownRemaining;
        private int _shownEnergy;
        private int _shownLevel;

        public event Action<int> OnBoostPressed;
        public event Action OnOverlayPressed;

        protected override void Initialize()
        {
            base.Initialize();
            _energyFill = energyBar.fillRect.GetComponent<Image>();
            _boostFill = boostBar.fillRect.GetComponent<Image>();
            _energyTint = _energyFill.color;
            for (var index = 0; index < boostButtons.Length; index++)
            {
                var level = index + MinLevel;
                boostButtons[index].onClick.AddListener(() => Press(level));
            }
            overlayButton.onClick.AddListener(() => OnOverlayPressed?.Invoke());
        }

        public void Bind(BoostSettings boost, RaceSettings race)
        {
            _settings = boost;
            _restingDial = race.baseSpeed * KilometresPerHour;
            _peakDial = _restingDial * MaxLevel;
            _dial = _restingDial;
            _energyFlash = 0f;
            _shownRank = 0;
            _shownSpeed = -1;
            _shownRemaining = -1;
            _shownEnergy = -1;
            _shownLevel = -1;
        }

        public void Refresh(IBoostState boost, int rank, int total, float speed, float travelled, float raceDistance)
        {
            progressBar.value = Mathf.Clamp01(travelled / raceDistance);
            energyBar.value = boost.EnergyRatio;
            boostBar.value = boost.IsActive ? boost.RemainingWindow / boost.WindowDuration : 0f;
            PaintEnergy(boost);

            if (rank != _shownRank)
            {
                var previous = _shownRank;
                _shownRank = rank;
                rankText.text = string.Format(RankFormat, rank, total);
                if (previous > 0) MarkRankChange(rank < previous);
            }
            RefreshDial(speed);
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
                boostText.text = string.Format(ActiveBoostFormat, boost.ActiveLevel,
                    BoostCurves.Describe(boost.Curve), boost.RemainingWindow);
            else if (_shownLevel != 0)
                boostText.text = ReadyLabel;
            if (boost.ActiveLevel != _shownLevel)
            {
                _shownLevel = boost.ActiveLevel;
                _boostFill.color = BoostPalette.ForLevel(_settings, Mathf.Max(_shownLevel, MinLevel));
            }
            RefreshBoostButtons(boost);
        }

        public void FlashAccepted(int level)
        {
            feedbackText.text = string.Format(AcceptedFormat, level);
            feedbackText.color = BoostPalette.ForLevel(_settings, level);
            _energyFlash = 1f;
            Pulse();
        }

        public void FlashRejected(BoostRequestOutcome outcome)
        {
            feedbackText.text = BoostFeedback.Describe(outcome);
            feedbackText.color = RejectColor;
            Pulse();
        }

        private void RefreshDial(float speed)
        {
            var target = speed * KilometresPerHour;
            _dial = Mathf.Lerp(_dial, target, DialLerp * Time.deltaTime);
            var rounded = Mathf.RoundToInt(_dial);
            if (rounded != _shownSpeed)
            {
                _shownSpeed = rounded;
                speedText.text = string.Format(SpeedFormat, rounded);
            }
            speedText.color = Color.Lerp(SpeedCalmColor, SpeedHotColor,
                Mathf.InverseLerp(_restingDial, _peakDial, _dial));
        }

        private void PaintEnergy(IBoostState boost)
        {
            _energyFlash = Mathf.MoveTowards(_energyFlash, 0f, EnergyFlashFade * Time.deltaTime);
            var lack = 1f - Mathf.InverseLerp(0f, LowEnergyRatio, boost.EnergyRatio);
            var pulse = Mathf.Abs(Mathf.Sin(Time.time * LowEnergyPulseRate)) * lack;
            _energyFill.color = Color.Lerp(Color.Lerp(_energyTint, LowEnergyColor, pulse),
                EnergyFlashColor, _energyFlash);
        }

        private void MarkRankChange(bool gained)
        {
            var token = this.GetCancellationTokenOnDestroy();
            rankCard.GoPunch(RankPulseScale, RankPulseDuration, token);
            rankText.color = gained ? RankGainColor : RankLossColor;
            rankText.GoTint(RankRestColor, RankTintDuration, token: token);
        }

        private void Press(int level)
        {
            boostButtons[level - MinLevel].image.rectTransform.GoPunch(KeyPunchScale, KeyPunchDuration,
                this.GetCancellationTokenOnDestroy());
            OnBoostPressed?.Invoke(level);
        }

        private void RefreshBoostButtons(IBoostState boost)
        {
            for (var index = 0; index < boostButtons.Length; index++)
            {
                var level = index + MinLevel;
                boostButtons[index].image.color = boost.ActiveLevel == level
                    ? BoostPalette.ForLevel(_settings, level)
                    : IsBlocked(boost, level) ? BlockedColor : ReadyColor;
            }
        }

        private static bool IsBlocked(IBoostState boost, int level) =>
            boost.IsActive || boost.RemainingCooldown > 0f || !boost.CanAfford(level);

        private void Pulse()
        {
            var token = this.GetCancellationTokenOnDestroy();
            feedbackGroup.alpha = 1f;
            feedbackGroup.GoFade(0f, FeedbackFadeDuration, token: token);
            feedbackText.rectTransform.GoPunch(FeedbackPunchScale, FeedbackPunchDuration, token);
        }
    }
}
