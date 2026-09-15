using Core.DI.Attributes;
using Core.UI.Interfaces;
using Game.Ai.Enums;
using Game.Manager.Interfaces;
using Game.Options.Interfaces;
using Game.Race.Interfaces;
using Game.UI.Views;
using UnityEngine;

namespace Game.States
{
    public class SetupState : RaceState
    {
        [Inject] private IUIManager _uiManager;
        [Inject] private IRaceOptions _options;
        [Inject] private IRaceManager _race;

        private readonly int _seed;
        private SetupView _view;

        public SetupState(IGameManager gameManager, int seed) : base(gameManager) => _seed = seed;

        public override void Enter()
        {
            _race.Prepare(_seed);
            _view = _uiManager.GetView<SetupView>();
            _view.OnRivalSelected += HandleRivals;
            _view.OnBalanceSelected += HandleBalance;
            _view.OnStartClicked += HandleStart;
            Refresh();
            _uiManager.Show<SetupView>(true);
        }

        public override void Tick() => _race.Present(Time.deltaTime);

        public override void Exit()
        {
            _view.OnRivalSelected -= HandleRivals;
            _view.OnBalanceSelected -= HandleBalance;
            _view.OnStartClicked -= HandleStart;
            _view.Hide();
        }

        private void HandleRivals(RivalMode mode)
        {
            _options.SetRivals(mode);
            Refresh();
        }

        private void HandleBalance(bool enabled)
        {
            _options.SetBalancing(enabled);
            Refresh();
        }

        private void Refresh() => _view.Refresh(_options.Rivals, _options.IsBalancingEnabled);

        private void HandleStart() => GameManager.ChangeState(new CountdownState(GameManager, _seed));
    }
}
