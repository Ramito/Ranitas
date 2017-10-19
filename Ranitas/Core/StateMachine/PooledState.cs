namespace Ranitas.Core.StateMachine
{
    public abstract class PooledState<TState, TStateMachine> : State<TStateMachine> where TStateMachine : StateMachine<TStateMachine> where TState : PooledState<TState, TStateMachine>, new()
    {
        private static int sPoolIndex;
        private static TState[] sPool;

        public static void SetupPool(int poolSize)
        {
            sPool = new TState[poolSize];
            sPoolIndex = poolSize;
            for (int i = 0; i < poolSize; ++i)
            {
                sPool[i] = new TState();
            }
        }

        public static TState Reserve()
        {
            --sPoolIndex;
            return sPool[sPoolIndex];
        }

        public static void ShutdownPool()
        {
            sPool = null;
        }

        public sealed override void OnExit(TStateMachine stateMachine)
        {
            OnExitAndPool(stateMachine);
            Return((TState)this);
        }

        protected abstract void OnExitAndPool(TStateMachine stateMachine);

        private static void Return(TState state)
        {
            System.Diagnostics.Debug.Assert(sPool[sPoolIndex] != null);
            sPool[sPoolIndex] = state;
            ++sPoolIndex;
        }
    }
}
