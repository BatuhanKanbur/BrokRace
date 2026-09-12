using Core.DI.Attributes;
using Core.UI.Interfaces;
using Game.Configuration.Interfaces;
using Game.Input.Interfaces;
using Game.Manager.Interfaces;
using Game.Race.Interfaces;
using Game.Telemetry.Interfaces;
using Game.UI.Views;
using UnityEngine;
using static Game.Race.Constants.RaceConstants;

namespace Game.States
{
    public class ResultState : RaceState
    {
        [Inject] private IRaceManager _race;
        [Inject] private IRaceState _raceState;
        [Inject] private IUIManager _uiManager;
        [Inject] private ITelemetryWriter _writer;
        [Inject] private IRaceConfigService _configService;

        private readonly int _seed;
        private readonly IBoostInputSource _input;
        private ResultView _view;

        public ResultState(IGameManager gameManager, int seed, IBoostInputSource input) : base(gameManager)
        {
            _seed = seed;
            _input = input;
        }

        public override void Enter()
        {
            _view = _uiManager.GetView<ResultView>();
            _view.ShowResults(_race.Order.Order, _raceState.Player);
            _view.OnRestartClicked += HandleRestart;
            _uiManager.Show<ResultView>();
            var report = _race.BuildReport(LiveRunPrefix, Application.targetFrameRate);
            _writer.Write(_race.Log, report, $"{LiveRunPrefix}_{_seed}");
        }

        public override void Tick() => _race.Present(Time.deltaTime);

        public override void Exit()
        {
            _view.OnRestartClicked -= HandleRestart;
            _view.Hide();
        }

        private void HandleRestart()
        {
            var settings = _configService.Config.race;
            var seed = settings.randomizeSeed ? Random.Range(1, int.MaxValue) : settings.seed;
            GameManager.ChangeState(new CountdownState(GameManager, seed, _input));
        }
    }
}
