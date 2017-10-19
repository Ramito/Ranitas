using Ranitas.Core;
using Ranitas.Core.StateMachine;
using Ranitas.Data;

namespace Ranitas.Frog.Sim
{
    public sealed class FrogToungue : StateMachine<FrogToungue>
    {
        public float TimeStep;
        public readonly FrogData Data;
        public bool ExtendSignal = false;

        public bool ToungueActive;
        public float RelativePhase { get { return MathExtensions.Clamp01(StatePhase / StateDuration); } }
        public float RelativeLength;

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
                stateMachine.ToungueActive = ToungueActive(stateMachine);
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
                    else
                    {
                        UpdateLength(stateMachine);
                    }
                }
                else
                {
                    stateMachine.StatePhase += stateMachine.TimeStep;
                    UpdateLength(stateMachine);
                }
            }

            public sealed override void OnExit(FrogToungue stateMachine) { }

            protected abstract float GetStateDuration(FrogData data);

            protected abstract bool TransitionOnFullPhase(FrogToungue stateMachine);

            protected abstract State<FrogToungue> GetNextTransitionState(FrogToungue stateMachine);

            protected virtual bool ToungueActive(FrogToungue stateMachine) { return false; }

            protected virtual float ToungueRelativeLength(FrogToungue stateMachine) { return 0f; }

            private void UpdateLength(FrogToungue stateMachine)
            {
                stateMachine.RelativeLength = MathExtensions.Clamp01(ToungueRelativeLength(stateMachine));
            }
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

            protected sealed override bool ToungueActive(FrogToungue stateMachine)
            {
                return true;
            }

            protected sealed override float ToungueRelativeLength(FrogToungue stateMachine)
            {
                return stateMachine.RelativePhase;
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

            protected sealed override bool ToungueActive(FrogToungue stateMachine)
            {
                return true;
            }

            protected sealed override float ToungueRelativeLength(FrogToungue stateMachine)
            {
                return 1f;
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

            protected sealed override bool ToungueActive(FrogToungue stateMachine)
            {
                return false;
            }

            protected sealed override float ToungueRelativeLength(FrogToungue stateMachine)
            {
                return 1f - stateMachine.RelativePhase;
            }
        }
    }
}
