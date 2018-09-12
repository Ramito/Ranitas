using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Core.EventSystem;
using System;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public struct FrogJumpData
    {
        public FrogJumpData(Data.FrogData data)
        {
            JumpPrepareTime = data.MovementData.JumpPrepareTime;
            JumpSpeed = data.MovementData.JumpVelocity;
        }

        public float JumpPrepareTime;
        public float JumpSpeed;
    }

    public sealed class FrogInputSystem : ISystem
    {
        private const float kMinMagnitude = 0.15f;
        private const float kMaxMagnitude = 0.85f;
        private const float kRangeNormalizer = 1f / (kMaxMagnitude - kMinMagnitude);
        //TODO: Data drive ^^^

        private static double[] sBlessedAngles = new double[] { Math.PI / 3d, (Math.PI / 3d) + (Math.PI / 24d), (Math.PI / 3d) + (2 * Math.PI / 24d), (Math.PI / 3d) + (3 * Math.PI / 24d) };
        private static readonly Vector2[] sBlessedDirections;

        static FrogInputSystem()
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

        public FrogInputSystem(FrameTime time, Data.FrogData frogData)
        {
            mTime = time;
            mJumpData = new FrogJumpData(frogData);
        }

        private FrameTime mTime;
        private FrogJumpData mJumpData;

        private struct PlayersSlice
        {
            public SliceRequirementOutput<Player> Player;
            public SliceRequirementOutput<ControlledEntity> ControlledEntity;
        }
        private PlayersSlice mPlayersSlice;

        private struct LandedFrogs
        {
            public SliceEntityOutput Frogs;
            public SliceRequirementOutput<Landed> Landed;
            public SliceRequirementOutput<FrogControlState> ControlState;
        }
        private LandedFrogs mGroundedFrogs;
        private List<Entity> mJumpingFrogs = new List<Entity>(4);

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mPlayersSlice);
            registry.SetupSlice(ref mGroundedFrogs);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            UpdateControlState(registry);
            UpdateLandedFrogs(registry);
        }

        private void UpdateControlState(EntityRegistry registry)
        {
            int count = mPlayersSlice.Player.Count;
            for (int i = 0; i < count; ++i)
            {
                int playerIndex = mPlayersSlice.Player[i].Index;
                GamePadState state = GamePad.GetState(playerIndex);

                FrogControlState controlState = new FrogControlState();
                controlState.JumpSignal = state.IsButtonDown(Buttons.A);
                controlState.ToungueSignalState = state.IsButtonDown(Buttons.X);
                Vector2 direction = state.ThumbSticks.Left;
                float rawMagnitude = direction.Length();
                if (rawMagnitude >= kMinMagnitude)
                {
                    direction.Normalize();
                }
                else
                {
                    direction = Vector2.Zero;
                }
                controlState.InputDirection = direction;

                Entity controlledFrog = mPlayersSlice.ControlledEntity[i].Entity;
                registry.SetComponent(controlledFrog, controlState);
            }
        }

        private void UpdateLandedFrogs(EntityRegistry registry)
        {
            int count = mGroundedFrogs.Landed.Count;
            for (int i = 0; i < count; ++i)
            {
                Entity frog = mGroundedFrogs.Frogs[i];
                Landed landed = mGroundedFrogs.Landed[i];
                FrogControlState controlState = mGroundedFrogs.ControlState[i];
                if (controlState.InputDirection.X != 0)
                {
                    landed.FacingDirection = Math.Sign(controlState.InputDirection.X);
                }
                if (controlState.InputDirection.Y >= 0f)
                {
                    if (controlState.JumpSignal)
                    {
                        float newJumpPercentage = landed.RelativeJumpPower + (mTime.DeltaTime / mJumpData.JumpPrepareTime);
                        landed.RelativeJumpPower = MathExtensions.Clamp01(newJumpPercentage);
                    }
                    else if (landed.RelativeJumpPower != 0f)
                    {
                        Vector2 blessedDirection = BestBlessedDirection(controlState.InputDirection, landed.FacingDirection);
                        Vector2 jumpVelocity = (landed.RelativeJumpPower * mJumpData.JumpSpeed) * blessedDirection;
                        registry.SetComponent(frog, new Velocity(jumpVelocity));
                        mJumpingFrogs.Add(frog);
                    }
                }
                else
                {
                    landed.RelativeJumpPower = 0f;
                }
                registry.SetComponent(frog, landed);
            }
            foreach (Entity frog in mJumpingFrogs)
            {
                registry.RemoveComponent<Landed>(frog);
            }
            mJumpingFrogs.Clear();
        }

        private static Vector2 BestBlessedDirection(Vector2 direction, int defaultDirection)
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
                blessedDir.X *= Math.Sign(defaultDirection);
            }
            if (direction.Y != 0f)
            {
                blessedDir.Y *= Math.Sign(direction.Y);
            }
            return blessedDir;
        }
    }
}
