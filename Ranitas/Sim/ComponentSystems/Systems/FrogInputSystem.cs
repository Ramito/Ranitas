﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Ranitas.Core;
using Ranitas.Core.ECS;
using System;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class FrogInputSystem : ISystem
    {
        private const float kMinMagnitude = 0.15f;
        private const float kMaxMagnitude = 0.85f;
        private const float kRangeNormalizer = 1f / (kMaxMagnitude - kMinMagnitude);
        //TODO: Data drive ^^^

        private static double[] sBlessedAngles = new double[] { Math.PI / 3d, (Math.PI / 3d) + (Math.PI / 8d) };
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
            mSwimData = new FrogSwimData(frogData);
        }

        private FrameTime mTime;
        private FrogJumpData mJumpData;
        private FrogSwimData mSwimData;

        private struct PlayersSlice
        {
            public SliceRequirementOutput<Player> Player;
            public SliceRequirementOutput<ControlledEntity> ControlledEntity;
        }
        private PlayersSlice mPlayersSlice;

        private struct LandedFrogs
        {
            public SliceEntityOutput Frogs;
            public SliceRequirementOutput<Facing> Facing;   //Better to get from registry if rare or add it here?
            public SliceRequirementOutput<Landed> Landed;
            public SliceRequirementOutput<FrogControlState> ControlState;
        }
        private LandedFrogs mLandedFrogs;
        private List<Entity> mJumpingFrogs = new List<Entity>(4);

        private struct NoToungueFrogs
        {
            public SliceEntityOutput Frogs;
            public SliceRequirementOutput<Facing> Facing;
            public SliceRequirementOutput<FrogControlState> ControlState;
            public SliceExclusion<ControlledEntity> NoToungue;
        }
        private NoToungueFrogs mNoToungueFrogs;

        private struct WaterborneFrogs
        {
            public SliceEntityOutput Frogs;
            public SliceRequirementOutput<Waterborne> Waterborne;
            public SliceRequirementOutput<FrogControlState> ControlState;
        }
        private WaterborneFrogs mWaterborneFrogs;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mPlayersSlice);
            registry.SetupSlice(ref mLandedFrogs);
            registry.SetupSlice(ref mNoToungueFrogs);
            registry.SetupSlice(ref mWaterborneFrogs);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            UpdateControlState(registry);
            UpdateNoToungueFrogs(registry);
            UpdateLandedFrogs(registry);
            UpdateSwimingFrogs(registry);
        }

        private void UpdateControlState(EntityRegistry registry)
        {
            int count = mPlayersSlice.Player.Count;
            for (int i = 0; i < count; ++i)
            {
                int playerIndex = mPlayersSlice.Player[i].Index;
                GamePadState state = GamePad.GetState(playerIndex);
                FrogControlState controlState = new FrogControlState();
                Vector2 direction = Vector2.Zero;
                if (state.IsConnected)
                {
                    controlState.JumpSignal = state.IsButtonDown(Buttons.A);
                    controlState.ToungueSignalState = state.IsButtonDown(Buttons.X);
                    direction = state.ThumbSticks.Left;
                    if (direction == Vector2.Zero)
                    {
                        if (state.IsButtonDown(Buttons.DPadDown))
                        {
                            direction -= Vector2.UnitY;
                        }
                        if (state.IsButtonDown(Buttons.DPadUp))
                        {
                            direction += Vector2.UnitY;
                        }
                        if (state.IsButtonDown(Buttons.DPadLeft))
                        {
                            direction -= Vector2.UnitX;
                        }
                        if (state.IsButtonDown(Buttons.DPadRight))
                        {
                            direction += Vector2.UnitX;
                        }
                    }
                }
                else
                {
                    KeyboardState keyboardState = Keyboard.GetState();
                    if (keyboardState.IsKeyDown(Keys.Left))
                    {
                        direction.X -= 1.0f;
                    }
                    if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        direction.X += 1.0f;
                    }
                    if (keyboardState.IsKeyDown(Keys.Down))
                    {
                        direction.Y -= 1.0f;
                    }
                    if (keyboardState.IsKeyDown(Keys.Up))
                    {
                        direction.Y += 1.0f;
                    }
                    controlState.JumpSignal = keyboardState.IsKeyDown(Keys.LeftControl);
                    controlState.ToungueSignalState = keyboardState.IsKeyDown(Keys.Space);
                }
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

        private void UpdateNoToungueFrogs(EntityRegistry registry)
        {
            int count = mNoToungueFrogs.Frogs.Count;
            for (int i = 0; i < count; ++i)
            {
                FrogControlState controlState = mNoToungueFrogs.ControlState[i];
                if (controlState.InputDirection.X != 0)
                {
                    Facing facing = mNoToungueFrogs.Facing[i];
                    int sign = Math.Sign(controlState.InputDirection.X);
                    if (sign != facing.CurrentFacing)
                    {
                        registry.SetComponent(mNoToungueFrogs.Frogs[i], new Facing(sign));
                    }
                }
            }
        }

        private void UpdateLandedFrogs(EntityRegistry registry)
        {
            int count = mLandedFrogs.Frogs.Count;
            for (int i = 0; i < count; ++i)
            {
                Entity frog = mLandedFrogs.Frogs[i];
                Landed landed = mLandedFrogs.Landed[i];
                FrogControlState controlState = mLandedFrogs.ControlState[i];
                if (controlState.InputDirection.Y >= 0f)
                {
                    if (controlState.JumpSignal)
                    {
                        float newJumpPercentage = landed.RelativeJumpPower + (mTime.DeltaTime / mJumpData.JumpPrepareTime);
                        landed.RelativeJumpPower = MathExtensions.Clamp01(newJumpPercentage);
                    }
                    else if (landed.RelativeJumpPower != 0f)
                    {
                        Facing facing = mLandedFrogs.Facing[i]; //For rare cases, should I fetch this from the registry instead?
                        Vector2 blessedDirection = BestBlessedDirection(controlState.InputDirection, facing.CurrentFacing);
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

        private void UpdateSwimingFrogs(EntityRegistry registry)
        {
            int count = mWaterborneFrogs.Frogs.Count;
            for (int i = 0; i < count; ++i)
            {
                float swimKickPhase = mWaterborneFrogs.Waterborne[i].SwimKickPhase;
                if (swimKickPhase < 0f)
                {
                    swimKickPhase = swimKickPhase + mTime.DeltaTime;
                    if (swimKickPhase >= 0f)
                    {
                        swimKickPhase = mSwimData.SwimKickDuration;
                    }
                }
                else if ((swimKickPhase > 0f) && (mWaterborneFrogs.ControlState[i].InputDirection != Vector2.Zero))
                {
                    swimKickPhase = Math.Max(0f, swimKickPhase - mTime.DeltaTime);
                }
                else if ((swimKickPhase != mSwimData.SwimKickDuration) && (mWaterborneFrogs.ControlState[i].InputDirection == Vector2.Zero))
                {
                    swimKickPhase = -mSwimData.SwimKickRecharge;
                }
                //TODO: Worth to validate the data changed?
                Waterborne waterBorne = new Waterborne(swimKickPhase);
                registry.SetComponent(mWaterborneFrogs.Frogs[i], waterBorne);
            }
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
