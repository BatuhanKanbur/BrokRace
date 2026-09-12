using System.Text;
using Core.UI.Abstracts;
using Game.Race.Interfaces;
using Game.Standings.Interfaces;
using Game.Telemetry.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Game.UI.Constants.HudConstants;

namespace Game.UI.Views
{
    public class DebugOverlayView : BaseView
    {
        [SerializeField] private Text headerText;
        [SerializeField] private Text bodyText;

        private readonly StringBuilder _builder = new();
        private bool _visible;

        public void Refresh(IRaceState race, IRaceOrder order, ITelemetryRecorder telemetry)
        {
            if (Keyboard.current != null && Keyboard.current[ToggleKey].wasPressedThisFrame) Toggle();
            if (!_visible) return;
            var player = race.Player;
            headerText.text = $"seed {race.Seed}   t {race.Time:0.00}s   samples {telemetry.Samples.Count}";
            _builder.Clear();
            _builder.AppendLine(OverlayHeader);
            foreach (var car in order.Order)
            {
                _builder.AppendLine(
                    $"{order.RankOf(car)}  {car.DisplayName,-10} {car.Distance,8:0.0} {car.Speed,6:0.0} " +
                    $"{car.Distance - player.Distance,8:+0.0;-0.0;0.0} x{car.BoostState.ActiveLevel} " +
                    $"{car.BoostState.Energy,5:0} {car.BalanceScale,6:0.000}");
            }
            bodyText.text = _builder.ToString();
        }

        private void Toggle()
        {
            _visible = !_visible;
            if (_visible) Show(OverlayFadeDuration);
            else Hide(OverlayFadeDuration);
        }
    }
}
