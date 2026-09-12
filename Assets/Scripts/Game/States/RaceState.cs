using Core.FiniteStateMachine.Interfaces;
using Game.Manager.Interfaces;

namespace Game.States
{
    public abstract class RaceState : IState
    {
        protected readonly IGameManager GameManager;

        protected RaceState(IGameManager gameManager) => GameManager = gameManager;

        public abstract void Enter();
        public abstract void Tick();
        public abstract void Exit();
    }
}
