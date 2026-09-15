using Core.DI.Attributes;
using Core.UI.Interfaces;
using Game.Boost.Enums;
using Game.Input.Interfaces;
using Game.Manager.Interfaces;
using Game.Race.Interfaces;
using Game.UI.Views;
using UnityEngine;

namespace Game.States
{
    public class RacingState : RaceState
    {
        [Inject] private IRaceManager _race;
        [Inject] private IUIManager _uiManager;
        [Inject] private IBoostRequestSink _boost;

        private readonly int _seed;
        private RaceHudView _hud;
        private DebugOverlayView _overlay;

        public RacingState(IGameManager gameManager, int seed) : base(gameManager) => _seed = seed;

        public override void Enter()
        {
            _hud = _uiManager.GetView<RaceHudView>();
            _overlay = _uiManager.GetView<DebugOverlayView>();
            _hud.OnBoostPressed += _boost.Request;
            _hud.OnOverlayPressed += _overlay.Toggle;
            _race.OnBoostAccepted += HandleAccepted;
            _race.OnBoostRejected += HandleRejected;
            _race.Begin();
        }

        public override void Tick()
        {
            var frameTime = Time.deltaTime;
            _race.Tick(frameTime);
            _race.Present(frameTime);
            var player = _race.State.Player;
            _hud.Refresh(player.BoostState, _race.Order.RankOf(player), _race.State.Cars.Count,
                player.Speed, player.Distance, _race.State.RaceDistance);
            _overlay.Refresh(_race.State, _race.Order, _race.Log, _race.Balancer.IsSuspended);
            if (_race.HasCompleted)
                GameManager.ChangeState(new ResultState(GameManager, _seed));
        }

        public override void Exit()
        {
            _hud.OnBoostPressed -= _boost.Request;
            _hud.OnOverlayPressed -= _overlay.Toggle;
            _race.OnBoostAccepted -= HandleAccepted;
            _race.OnBoostRejected -= HandleRejected;
        }

        private void HandleAccepted(int level) => _hud.FlashAccepted(level);

        private void HandleRejected(int level, BoostRequestOutcome outcome) => _hud.FlashRejected(outcome);
    }
}
