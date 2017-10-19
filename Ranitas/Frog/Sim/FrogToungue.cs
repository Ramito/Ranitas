using Ranitas.Core.StateMachine;
using Ranitas.Data;

namespace Ranitas.Frog.Sim
{
    public sealed class FrogToungue : StateMachine<FrogToungue>
    {
        public float TimeStep;
        public readonly FrogData Data;
        public bool ExtendSignal = false;

        public float StateDuration;
        public float StatePhase;

        public FrogToungue(FrogData data)
        {
            Data = data;
            TransitionTo(Retracted.Instance);
        }

        public void Update(float deltaTime)
        {
            TimeStep = deltaTime;
            UpdateState();
        }

        private abstract class ToungueStateBase<TState> : State<FrogToungue> where TState : ToungueStateBase<TState>, new()
        {
            public readonly static TState Instance = new TState();

            public sealed override void OnEnter(FrogToungue stateMachine)
            {
                stateMachine.StateDuration = GetStateDuration(stateMachine.Data);
                stateMachine.StatePhase = 0f;
            }

            public sealed override void OnUpdate(FrogToungue stateMachine)
            {
                if (stateMachine.StatePhase >= stateMachine.StateDuration)
                {
                    if (TransitionOnFullPhase(stateMachine))
                    {
                        stateMachine.TransitionTo(GetNextTransitionState(stateMachine));
                    }
                }
                else
                {
                    stateMachine.StatePhase += stateMachine.TimeStep;
                }
            }

            public sealed override void OnExit(FrogToungue stateMachine) { }

            protected abstract float GetStateDuration(FrogData data);

            protected abstract bool TransitionOnFullPhase(FrogToungue stateMachine);

            protected abstract State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine);
        }

        private sealed class Retracted : ToungueStateBase<Retracted>
        {
            protected sealed override bool TransitionOnFullPhase(FrogToungue stateMachine)
            {
                return stateMachine.ExtendSignal;
            }

            protected sealed override State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine)
            {
                return Extending.Instance;
            }

            protected sealed override float GetStateDuration(FrogData data)
            {
                return data.ToungueRefreshTime;
            }
        }

        private sealed class Extending : ToungueStateBase<Extending>
        {
            protected sealed override bool TransitionOnFullPhase(FrogToungue stateMachine)
            {
                return true;
            }

            protected sealed override State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine)
            {
                return Extended.Instance;
            }

            protected sealed override float GetStateDuration(FrogData data)
            {
                return data.ToungueExtendTime;
            }
        }

        private sealed class Extended : ToungueStateBase<Extended>
        {
            protected sealed override State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine)
            {
                return Retracting.Instance;
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

        private sealed class Retracting : ToungueStateBase<Retracting>
        {
            protected sealed override State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine)
            {
                return Retracted.Instance;
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
    }
}
