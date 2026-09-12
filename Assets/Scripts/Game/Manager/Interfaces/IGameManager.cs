using Core.FiniteStateMachine.Interfaces;

namespace Game.Manager.Interfaces
{
    public interface IGameManager
    {
        public void ChangeState(IState newState);
    }
}
