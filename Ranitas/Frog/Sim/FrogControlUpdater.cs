using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.EventSystem;
using Ranitas.Data;
using Ranitas.Input;
using System;

namespace Ranitas.Frog.Sim
{
    public sealed class FrogControlUpdater
    {
        private static double[] sBlessedAngles = new double[] {Math.PI / 3d, (Math.PI / 3d) + (Math.PI / 24d), (Math.PI / 3d) + (2 * Math.PI / 24d), (Math.PI / 3d) + (3 * Math.PI / 24d) };
        private static readonly Vector2[] sBlessedDirections;

        static FrogControlUpdater()
        {
            int directionsCount = sBlessedAngles.Length;
            sBlessedDirections = new Vector2[directionsCount];
            int index = 0;
            foreach (double angle in sBlessedAngles)
            {
                sBlessedDirections[index] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                ++index;
            }
        }

        private readonly float mDeltaTime;
        private readonly EventSystem mEventSystem;

        public FrogControlUpdater(float deltaTime, EventSystem eventSystem)
        {
            mDeltaTime = deltaTime;
            mEventSystem = eventSystem;
        }

        public void Update(int id, FrogMovementData data, FrogControlState frogControlState, FrogInput input)
        {
            frogControlState.InputDirection = input.NormalizedDirection;
            if (frogControlState.InputDirection.X > 0f)
            {
                frogControlState.FacingDirection = 1;
            }
            else if (frogControlState.InputDirection.X < 0f)
            {
                frogControlState.FacingDirection = -1;
            }

            bool currentFrogSignal = frogControlState.ToungueSignalState;

            if (frogControlState.State != FrogControlState.States.Swimming)
            {
                if (frogControlState.State == FrogControlState.States.Grounded)
                {
                    if (input.NormalizedDirection.Y >= 0f)
                    {
                        if ((input.Signals & FrogSignals.Jump) != 0)
                        {
                            float newJumpPercentage = frogControlState.RelativeJumpPower + (mDeltaTime / data.JumpPrepareTime);
                            frogControlState.RelativeJumpPower = MathExtensions.Clamp01(newJumpPercentage);
                        }
                        else if (frogControlState.RelativeJumpPower != 0f)
                        {
                            Vector2 blessedDirection = BestBlessedDirection(input.NormalizedDirection, frogControlState.FacingDirection);
                            Vector2 jumpVelocity = (frogControlState.RelativeJumpPower * data.JumpVelocity) * blessedDirection;
                            mEventSystem.PostMessage(new JumpMessage(id, jumpVelocity));
                            frogControlState.RelativeJumpPower = 0f;
                        }
                    }
                    else
                    {
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
            
            if (!currentFrogSignal && frogControlState.ToungueSignalState)
            {
                mEventSystem.PostMessage(new ToungueMessage(id, frogControlState.FacingDirection));
            }
        }

        private static Vector2 BestBlessedDirection(Vector2 direction, int facing)
        {
            Vector2 absDirection = new Vector2(Math.Abs(direction.X), Math.Abs(direction.Y));
            float maxDot = float.MinValue;
            int bestIndex = -1;
            for (int i = sBlessedDirections.Length - 1; i >= 0; --i)
            {
                Vector2 blessedDirection = sBlessedDirections[i];
                float dot = Vector2.Dot(absDirection, blessedDirection);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestIndex = i;
                }
            }
            Vector2 blessedDir = sBlessedDirections[bestIndex];
            if (direction.X != 0f)
            {
                blessedDir.X *= Math.Sign(direction.X);
            }
            else
            {
                blessedDir.X *= Math.Sign(facing);
            }
            if (direction.Y != 0f)
            {
                blessedDir.Y *= Math.Sign(direction.Y);
            }
            return blessedDir;
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
