using Ranitas.Core;
using Ranitas.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ranitas.Frog.Sim
{
    public sealed class FrogToungue : StateMachine<FrogToungue>
    {
        public static float sTimeStep { get; private set; }
        public readonly FrogData Data;
        public float TimeStep { get { return sTimeStep; } }
        public bool ExtendSignal = false;

        public FrogToungue(FrogData data)
        {
            Data = data;
            TransitionTo(Retracted.Reserve());
        }

        public static void Initialize(float deltaTime, int maxNumberOfFrogs)
        {
            sTimeStep = deltaTime;
            Retracted.SetupPool(maxNumberOfFrogs);
            Extending.SetupPool(maxNumberOfFrogs);
            Extended.SetupPool(maxNumberOfFrogs);
            Retracting.SetupPool(maxNumberOfFrogs);
        }
    }

    public abstract class ToungueStateBase<TState> : PooledState<TState, FrogToungue> where TState : PooledState<TState, FrogToungue>, new()
    {
        private float mDuration;
        private float mPhase;

        public sealed override void OnEnter(FrogToungue stateMachine)
        {
            mDuration = GetStateDuration(stateMachine.Data);
            mPhase = 0f;
        }

        public sealed override void OnUpdate(FrogToungue stateMachine)
        {
            if (mPhase >= mDuration)
            {
                if (TransitionOnFullPhase(stateMachine))
                {
                    stateMachine.TransitionTo(GetNextTransitionState(stateMachine));
                }
            }
            else
            {
                mPhase += stateMachine.TimeStep;
            }
        }

        protected sealed override void OnExitAndPool(FrogToungue stateMachine)
        {
            mDuration = 0f;
            mPhase = 0f;
        }

        protected abstract float GetStateDuration(FrogData data);

        protected abstract bool TransitionOnFullPhase(FrogToungue stateMachine);

        protected abstract State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine);
    }

    public sealed class Retracted : ToungueStateBase<Retracted>
    {
        protected sealed override bool TransitionOnFullPhase(FrogToungue stateMachine)
        {
            return stateMachine.ExtendSignal;
        }

        protected sealed override State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine)
        {
            return Extending.Reserve();
        }

        protected sealed override float GetStateDuration(FrogData data)
        {
            return data.ToungueRefreshTime;
        }
    }

    public sealed class Extending : ToungueStateBase<Extending>
    {
        protected sealed override bool TransitionOnFullPhase(FrogToungue stateMachine)
        {
            return true;
        }

        protected sealed override State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine)
        {
            return Extended.Reserve();
        }

        protected sealed override float GetStateDuration(FrogData data)
        {
            return data.ToungueExtendTime;
        }
    }

    public sealed class Extended : ToungueStateBase<Extended>
    {
        protected sealed override State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine)
        {
            return Retracting.Reserve();
        }

        protected sealed override float GetStateDuration(FrogData data)
        {
            return data.ToungueFullyExtendedTime;
        }

        protected sealed override bool TransitionOnFullPhase(FrogToungue stateMachine)
        {
            return true;
        }
    }

    public sealed class Retracting : ToungueStateBase<Retracting>
    {
        protected sealed override State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine)
        {
            return Retracted.Reserve();
        }

        protected sealed override float GetStateDuration(FrogData data)
        {
            return data.ToungueRetractTime;
        }

        protected sealed override bool TransitionOnFullPhase(FrogToungue stateMachine)
        {
            return true;
        }
    }

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

        public void Update()
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

    public abstract class State<TStateMachine> where TStateMachine : StateMachine<TStateMachine>
    {
        public abstract void OnEnter(TStateMachine stateMachine);

        public abstract void OnExit(TStateMachine stateMachine);

        public abstract void OnUpdate(TStateMachine stateMachine);
    }

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
