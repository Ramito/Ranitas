namespace Ranitas.Core.StateMachine
{
    public abstract class State<TStateMachine> where TStateMachine : StateMachine<TStateMachine>
    {
        public abstract void OnEnter(TStateMachine stateMachine);

        public abstract void OnExit(TStateMachine stateMachine);

        public abstract void OnUpdate(TStateMachine stateMachine);
    }
}
