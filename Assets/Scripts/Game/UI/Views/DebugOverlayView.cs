using System.Collections.Generic;
using System.Text;
using Core.UI.Abstracts;
using Game.Ai.Interfaces;
using Game.Race.Interfaces;
using Game.Standings.Interfaces;
using Game.Telemetry.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Constants.HudConstants;

namespace Game.UI.Views
{
    public class DebugOverlayView : BaseView
    {
        [SerializeField] private Text headerText;
        [SerializeField] private Text bodyText;

        private readonly StringBuilder _builder = new();
        private readonly Dictionary<int, IAiDriver> _drivers = new();

        public bool IsOpen { get; private set; }

        public void Bind(IReadOnlyList<IAiDriver> drivers)
        {
            _drivers.Clear();
            foreach (var driver in drivers)
                _drivers[driver.CarIndex] = driver;
        }

        public void Toggle()
        {
            IsOpen = !IsOpen;
            if (IsOpen) Show(OverlayFadeDuration);
            else Hide(OverlayFadeDuration);
        }

        public void Refresh(IRaceState race, IRaceOrder order, ITelemetryLog log, bool balancingSuspended)
        {
            if (!IsOpen) return;
            var player = race.Player;
            headerText.text = string.Format(OverlayHeaderFormat, race.Seed, race.Time, race.LogicStep * MillisPerSecond,
                log.Samples.Count, balancingSuspended ? SuspendedLabel : ActiveLabel);
            _builder.Clear();
            _builder.AppendLine(OverlayColumns);
            foreach (var car in order.Order)
            {
                _builder.AppendLine(string.Format(OverlayRowFormat, order.RankOf(car), car.DisplayName,
                    car.Distance, car.Speed, car.Distance - player.Distance, car.BoostState.ActiveLevel,
                    car.BoostState.Energy, car.BalanceScale, car.AssistDistance, StateOf(car.Index)));
            }
            bodyText.text = _builder.ToString();
        }

        private string StateOf(int carIndex)
        {
            if (!_drivers.TryGetValue(carIndex, out var driver)) return PlayerLabel;
            var decision = driver.LastDecision;
            return string.Format(OverlayStateFormat, driver.ProfileName, decision.Level, decision.Strike,
                decision.Defend, decision.Pace, decision.Closing, decision.Spill, decision.Score, decision.HoldScore);
        }
    }
}
