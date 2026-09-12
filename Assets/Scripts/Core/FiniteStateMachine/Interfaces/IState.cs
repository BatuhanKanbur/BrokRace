namespace Core.FiniteStateMachine.Interfaces
{
    public interface IState
    {
        void Enter();
        void Tick();
        void Exit();
    }
}