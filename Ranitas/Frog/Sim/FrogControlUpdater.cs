using Microsoft.Xna.Framework;
using Ranitas.Core.EventSystem;
using Ranitas.Data;
using Ranitas.Input;

namespace Ranitas.Frog.Sim
{
    public sealed class FrogControlUpdater
    {
        private readonly float mDeltaTime;
        private readonly EventSystem mEventSystem;

        public FrogControlUpdater(float deltaTime, EventSystem eventSystem)
        {
            mDeltaTime = deltaTime;
            mEventSystem = eventSystem;
        }

        public void Update(int id, FrogMovementData data, FrogControlState frogControlState, FrogInput input)
        {
            bool currentFrogSignal = frogControlState.ToungueSignalState;
            if (frogControlState.State != FrogControlState.States.Swimming)
            {
                float jumpInputMagnitude = 0f;
                if (frogControlState.State == FrogControlState.States.Grounded)
                {
                    if (input.NormalizedDirection.Y >= 0.7f)
                    {
                        jumpInputMagnitude = input.Magnitude;
                    }
                }
                float newJumpPercentage = frogControlState.RelativeJumpPower + mDeltaTime / (data.JumpPrepareTime);
                frogControlState.RelativeJumpPower = MathHelper.Clamp(newJumpPercentage, 0f, jumpInputMagnitude);
                if (jumpInputMagnitude != 0)
                {
                    if ((input.Signals & FrogSignals.Jump) != 0)
                    {
                        Vector2 jumpVelocity = (frogControlState.RelativeJumpPower * data.JumpVelocity) * input.NormalizedDirection;
                        mEventSystem.PostMessage(new JumpMessage(id, jumpVelocity));
                        frogControlState.RelativeJumpPower = 0f;
                    }
                }
                frogControlState.ToungueSignalState = ((input.Signals & FrogSignals.Toungue) != 0);
            }
            else
            {
                frogControlState.ToungueSignalState = false;
                frogControlState.RelativeJumpPower = 0f;
            }

            frogControlState.InputDirection = input.NormalizedDirection;

            if (frogControlState.InputDirection.X > 0f)
            {
                frogControlState.ToungueDirection = 1;
            }
            else if (frogControlState.InputDirection.X < 0f)
            {
                frogControlState.ToungueDirection = -1;
            }
            if (!currentFrogSignal && frogControlState.ToungueSignalState)
            {
                mEventSystem.PostMessage(new ToungueMessage(id, frogControlState.ToungueDirection));
            }
        }
    }

    public struct JumpMessage
    {
        public readonly int ID;
        public readonly Vector2 Velocity;

        public JumpMessage(int id, Vector2 velocity)
        {
            ID = id;
            Velocity = velocity;
        }
    }

    public struct ToungueMessage
    {
        public readonly int ID;
        public readonly int Direction;

        public ToungueMessage(int id, int direction)
        {
            ID = id;
            Direction = direction;
        }
    }
}
