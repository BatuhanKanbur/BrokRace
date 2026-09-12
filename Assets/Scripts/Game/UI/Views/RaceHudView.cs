using Core.UI.Abstracts;
using Core.Utilities;
using Cysharp.Threading.Tasks;
using Game.Boost.Enums;
using Game.Boost.Interfaces;
using Game.Utilities;
using UnityEngine;
using UnityEngine.UI;
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

        public void Refresh(IBoostState boost, int rank, int total, float speed, float travelled, float raceDistance)
        {
            rankText.text = $"{rank}/{total}";
            speedText.text = $"{speed:0} m/s";
            remainingText.text = $"{Mathf.Max(0f, raceDistance - travelled):0} m";
            progressFill.fillAmount = Mathf.Clamp01(travelled / raceDistance);
            energyFill.fillAmount = boost.EnergyRatio;
            energyText.text = $"{boost.Energy:0}";
            boostFill.fillAmount = boost.IsActive ? boost.RemainingWindow / BoostWindowReference : 0f;
            boostFill.color = BoostPalette.ForLevel(boost.ActiveLevel);
            boostText.text = boost.IsActive ? $"x{boost.ActiveLevel}  {boost.RemainingWindow:0.00}s" : ReadyLabel;
            RefreshKeys(boost);
        }

        public void FlashAccepted(int level)
        {
            feedbackText.text = $"BOOST x{level}";
            feedbackText.color = BoostPalette.ForLevel(level);
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
                var level = index + 1;
                keySlots[index].color = boost.ActiveLevel == level
                    ? BoostPalette.ForLevel(level)
                    : Blocked(boost, level) ? BlockedColor : ReadyColor;
            }
        }

        private static bool Blocked(IBoostState boost, int level) =>
            boost.IsActive || boost.RemainingCooldown > 0f || !boost.CanAfford(level);

        private void Pulse()
        {
            feedbackGroup.alpha = 1f;
            feedbackGroup.GoFade(0f, FeedbackFadeDuration, token: this.GetCancellationTokenOnDestroy());
        }
    }
}
