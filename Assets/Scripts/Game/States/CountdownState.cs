using Core.DI.Attributes;
using Core.UI.Interfaces;
using Game.Configuration.Interfaces;
using Game.Input.Interfaces;
using Game.Manager.Interfaces;
using Game.Race.Interfaces;
using Game.UI.Views;
using UnityEngine;
using static Game.UI.Constants.HudConstants;

namespace Game.States
{
    public class CountdownState : RaceState
    {
        [Inject] private IRaceManager _race;
        [Inject] private IRaceConfigService _configService;
        [Inject] private IUIManager _uiManager;

        private readonly int _seed;
        private readonly IBoostInputSource _input;
        private CountdownView _view;
        private float _remaining;
        private int _shown;

        public CountdownState(IGameManager gameManager, int seed, IBoostInputSource input) : base(gameManager)
        {
            _seed = seed;
            _input = input;
        }

        public override void Enter()
        {
            _race.Prepare(_seed, _input);
            _remaining = _configService.Config.race.countdownSeconds;
            _shown = -1;
            _view = _uiManager.GetView<CountdownView>();
            _view.SetHint(CountdownHint);
            _uiManager.GetView<RaceHudView>().Bind(_configService.Config.boost);
            _uiManager.GetView<DebugOverlayView>().Bind(_race.Drivers);
            _uiManager.Show<CountdownView>();
            _uiManager.Show<RaceHudView>();
        }

        public override void Tick()
        {
            _remaining -= Time.deltaTime;
            var count = Mathf.CeilToInt(_remaining);
            if (count != _shown)
            {
                _shown = count;
                _view.SetCount(count);
            }
            _race.PumpInput();
            _race.Present(Time.deltaTime);
            if (_remaining > 0f) return;
            GameManager.ChangeState(new RacingState(GameManager, _seed, _input));
        }

        public override void Exit() => _view.Hide();
    }
}
