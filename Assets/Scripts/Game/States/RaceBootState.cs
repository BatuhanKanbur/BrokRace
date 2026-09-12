using Core.DI.Attributes;
using Cysharp.Threading.Tasks;
using Game.Configuration.Interfaces;
using Game.Input.Managers;
using Game.Manager.Interfaces;
using Game.Race.Interfaces;
using UnityEngine;

namespace Game.States
{
    public class RaceBootState : RaceState
    {
        [Inject] private IRaceManager _race;
        [Inject] private IRaceConfigService _configService;

        public RaceBootState(IGameManager gameManager) : base(gameManager) { }

        public override void Enter() => Boot().Forget();

        public override void Tick() { }

        public override void Exit() { }

        private async UniTaskVoid Boot()
        {
            await _race.Build();
            var settings = _configService.Config.race;
            var seed = settings.randomizeSeed ? Random.Range(1, int.MaxValue) : settings.seed;
            GameManager.ChangeState(new CountdownState(GameManager, seed, new KeyboardBoostInput()));
        }
    }
}
