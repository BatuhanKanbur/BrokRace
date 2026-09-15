using Core.DI.Attributes;
using Core.UI.Interfaces;
using Game.Configuration.Interfaces;
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
        [Inject] private IUIManager _uiManager;
        [Inject] private ITelemetryWriter _writer;
        [Inject] private IRaceConfigService _configService;

        private readonly int _seed;
        private ResultView _view;

        public ResultState(IGameManager gameManager, int seed) : base(gameManager) => _seed = seed;

        public override void Enter()
        {
            _view = _uiManager.GetView<ResultView>();
            _view.ShowResults(_race.Order.Order, _race.State.Player);
            _view.OnRestartClicked += HandleRestart;
            _view.OnModesClicked += HandleModes;
            _uiManager.Show<ResultView>(true);
            var report = _race.BuildReport(LiveRunPrefix, Application.targetFrameRate);
            _writer.Write(_race.Log, report, $"{LiveRunPrefix}_{_seed}");
        }

        public override void Tick() => _race.Present(Time.deltaTime);

        public override void Exit()
        {
            _view.OnRestartClicked -= HandleRestart;
            _view.OnModesClicked -= HandleModes;
            _view.Hide();
        }

        private void HandleRestart() => GameManager.ChangeState(new CountdownState(GameManager, NextSeed()));

        private void HandleModes() => GameManager.ChangeState(new SetupState(GameManager, NextSeed()));

        private int NextSeed()
        {
            var settings = _configService.Config.race;
            return settings.randomizeSeed ? Random.Range(1, int.MaxValue) : settings.seed;
        }
    }
}
