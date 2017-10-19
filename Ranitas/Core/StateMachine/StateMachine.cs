namespace Ranitas.Core.StateMachine
{
    public abstract class StateMachine<TStateMachine> where TStateMachine : StateMachine<TStateMachine>
    {
        private class IdleState : State<TStateMachine>
        {
            public override void OnEnter(TStateMachine stateMachine) { }

            public override void OnExit(TStateMachine stateMachine) { }

            public override void OnUpdate(TStateMachine stateMachine) { }
        }

        private readonly static IdleState sIdleState = new StateMachine<TStateMachine>.IdleState();

        private State<TStateMachine> mCurrentState = sIdleState;

        protected void UpdateState()
        {
            mCurrentState.OnUpdate((TStateMachine)this);
        }

        public void TransitionTo(State<TStateMachine> newState)
        {
            mCurrentState.OnExit((TStateMachine)this);
            mCurrentState = newState;
            mCurrentState.OnEnter((TStateMachine)this);
        }

        public void SetIdle()
        {
            TransitionTo(sIdleState);
        }
    }
}
