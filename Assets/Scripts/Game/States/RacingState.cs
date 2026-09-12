using Core.DI.Attributes;
using Core.UI.Interfaces;
using Game.Boost.Enums;
using Game.Configuration.Interfaces;
using Game.Input.Interfaces;
using Game.Input.Managers;
using Game.Manager.Interfaces;
using Game.Race.Interfaces;
using Game.UI.Views;
using UnityEngine;

namespace Game.States
{
    public class RacingState : RaceState
    {
        [Inject] private IRaceManager _race;
        [Inject] private IRaceState _raceState;
        [Inject] private IUIManager _uiManager;

        private readonly IDebugInput _debugInput = new DebugToggleInput();
        private readonly int _seed;
        private readonly IBoostInputSource _input;
        private RaceHudView _hud;
        private DebugOverlayView _overlay;

        public RacingState(IGameManager gameManager, int seed, IBoostInputSource input) : base(gameManager)
        {
            _seed = seed;
            _input = input;
        }

        public override void Enter()
        {
            _hud = _uiManager.GetView<RaceHudView>();
            _overlay = _uiManager.GetView<DebugOverlayView>();
            _race.OnBoostAccepted += HandleAccepted;
            _race.OnBoostRejected += HandleRejected;
            _race.Begin();
        }

        public override void Tick()
        {
            var frameTime = Time.deltaTime;
            if (_debugInput.ConsumeToggle()) _overlay.Toggle();
            _race.Tick(frameTime);
            _race.Present(frameTime);
            var player = _raceState.Player;
            _hud.Refresh(player.BoostState, _race.Order.RankOf(player), _raceState.Cars.Count,
                player.Speed, player.Distance, _raceState.RaceDistance);
            _overlay.Refresh(_raceState, _race.Order, _race.Log, _race.Balancer.IsSuspended);
            if (_race.HasCompleted)
                GameManager.ChangeState(new ResultState(GameManager, _seed, _input));
        }

        public override void Exit()
        {
            _race.OnBoostAccepted -= HandleAccepted;
            _race.OnBoostRejected -= HandleRejected;
        }

        private void HandleAccepted(int level) => _hud.FlashAccepted(level);

        private void HandleRejected(int level, BoostRequestOutcome outcome) => _hud.FlashRejected(outcome);
    }
}
