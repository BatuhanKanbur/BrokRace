using Core.DI.Managers;
using Core.FiniteStateMachine.Interfaces;
using Core.FiniteStateMachine.Structure;
using Game.Manager.Interfaces;
using UnityEngine;

namespace Game.Manager.Managers
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        private readonly StateMachine _stateMachine = new();

        private void Update() => _stateMachine.Tick();

        public void ChangeState(IState newState)
        {
            DiContainer.Inject(newState);
            _stateMachine.ChangeState(newState);
        }
    }
}
