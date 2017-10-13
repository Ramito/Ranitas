using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Frog;
using Ranitas.Pond;
using System;
using System.Collections.Generic;
using static Ranitas.Frog.FrogSimState;

namespace Ranitas.Sim
{
    public sealed class RanitasSim
    {
        public readonly static Vector2 kGravity = new Vector2(0f, -980f);    //TODO: Data drive!

        private PondSimState mPondState;
        private List<FrogSimState> mFrogStates;

        private FixedTimeStepDynamics mDynamics;

        private List<Rect> mFrameLilyRects;

        public RanitasSim(PondSimState pondState, List<FrogSimState> frogStates, float fixedTimeStep)
        {
            mPondState = pondState;
            mFrogStates = frogStates;
            mDynamics = new FixedTimeStepDynamics(fixedTimeStep);
            mFrameLilyRects = new List<Rect>(pondState.Lilies.Count);
        }

        public void Update()
        {
            UpdateLilies();
            UpdateFrogs();
        }

        private void UpdateLilies()
        {
            mFrameLilyRects.Clear();
            foreach (var lilyState in mPondState.Lilies)
            {
                Rect lilyRect = new Rect(lilyState.Position, lilyState.Width, lilyState.Height);
                mFrameLilyRects.Add(lilyRect);
            }
        }

        private void UpdateFrogs()
        {
            foreach (var frog in mFrogStates)
            {
                UpdateFrog(frog);
            }
        }

        private void UpdateFrog(FrogSimState frog)
        {
            if (frog.PreparingJump)
            {
                frog.TimePreparingJump += mDynamics.FixedTimeStep;
            }
            if (frog.State != FrogState.Swimming)
            {
                if ((frog.JumpSignaled) && (frog.State == FrogState.Grounded))
                {
                    Vector2 direction = frog.SignaledJumpDirection;
                    if (direction != Vector2.Zero)
                    {
                        direction.Normalize();
                    }
                    else
                    {
                        direction = Vector2.UnitY;
                    }
                    Vector2 velocity = (frog.PreparedJumpPercentage * frog.Prototype.JumpVelocity) * direction;
                    frog.Velocity += velocity;
                    ResetFrogInput(frog);
                }
                UpdateDryFrogDynamics(frog);
            }
            else
            {
                ResetFrogInput(frog);
                UpdateSwimingFrogDynamics(frog);
            }
        }

        private void ResetFrogInput(FrogSimState frog)
        {
            frog.PreparingJump = false;
            frog.JumpSignaled = false;
            frog.TimePreparingJump = 0f;
        }

        private void UpdateDryFrogDynamics(FrogSimState frog)
        {
            Vector2 prospectDelta = mDynamics.FramePositionDelta(kGravity, frog.Velocity);
            Vector2 prospectPosition = frog.Position + prospectDelta;
            if (frog.PreparingJump)
            {
                float relativeSquish = Math.Min(1f, frog.TimePreparingJump / frog.Prototype.JumpPrepareTime);
                float scale = relativeSquish * frog.Prototype.JumpSquish + (1f - relativeSquish);
                frog.Height = scale * frog.Prototype.Height;
                frog.Width = frog.Prototype.Width / scale;
            }
            else
            {
                frog.Height = frog.Prototype.Height;
                frog.Width = frog.Prototype.Width;
            }
            if (frog.Velocity.Y <= 0f)
            {
                Rect prospectRect = new Rect(prospectPosition, frog.Width, frog.Height);
                for (int i = 0; i < mFrameLilyRects.Count; ++i)
                {
                    Rect lilyRect = mFrameLilyRects[i];
                    if (prospectRect.Intersects(lilyRect))
                    {
                        //Assuming only one collision is possible!
                        float landingY = lilyRect.MaxY;
                        Vector2 inheritedVelocity = mPondState.Lilies[i].Velocity;
                        frog.FeetPosition = new Vector2(prospectPosition.X, landingY);
                        frog.Velocity = inheritedVelocity;
                        frog.State = FrogState.Grounded;
                        return;
                    }
                }
            }
            frog.Position = prospectPosition;
            frog.Velocity = frog.Velocity + mDynamics.FrameVelocityDelta(kGravity);
            if (frog.FeetPosition.Y < mPondState.WaterLevel)
            {
                frog.SwimKickPhase = -frog.Prototype.SwimKickRecharge;
                frog.State = FrogState.Swimming;
            }
            else
            {
                frog.State = FrogState.Airborne;
            }
        }

        private void UpdateSwimingFrogDynamics(FrogSimState frog)
        {
            Vector2 buouyancy = ComputeBuouyancyAcceleration(frog);
            Vector2 swimAcceleration = UpdateFrogSwimAndGetAcceleration(frog);
            Vector2 totalAcceperation = buouyancy + swimAcceleration;
            if ((frog.Velocity.Y > 0f) && (frog.Position.Y + (0.5f * frog.Height)) > mPondState.WaterLevel)
            {
                //No vertical drag!
                Vector2 verticalVelocity = new Vector2(0f, frog.Velocity.Y);
                Vector2 verticalAcceleration = new Vector2(0f, totalAcceperation.Y);
                frog.Position += mDynamics.FramePositionDelta(verticalAcceleration, verticalVelocity);
                Vector2 totalVerticalVelocity = verticalVelocity + mDynamics.FrameVelocityDelta(verticalAcceleration);

                Vector2 horizontalVelocity = new Vector2(frog.Velocity.X, 0f);
                Vector2 horizontalAcceleration = new Vector2(totalAcceperation.X, 0f);
                UpdateFrogDragDynamics(frog, horizontalVelocity, horizontalAcceleration);
                frog.Velocity += totalVerticalVelocity;
            }
            else
            {
                UpdateFrogDragDynamics(frog, frog.Velocity, totalAcceperation);
            }
            if (frog.FeetPosition.Y >= mPondState.WaterLevel)
            {
                frog.State = FrogState.Airborne;
            }
        }

        private void UpdateFrogDragDynamics(FrogSimState frog, Vector2 updateVelocity, Vector2 accelerationVector)
        {
            frog.Position += mDynamics.FrameLinearDragPositionDelta(updateVelocity, frog.Prototype.WaterDrag, accelerationVector);
            frog.Velocity = mDynamics.FrameLinearDragVelocity(updateVelocity, frog.Prototype.WaterDrag, accelerationVector);
        }

        private Vector2 UpdateFrogSwimAndGetAcceleration(FrogSimState frog)
        {
            if (frog.SwimKickPhase < 0f)
            {
                float newPhase = frog.SwimKickPhase + mDynamics.FixedTimeStep;
                if (newPhase >= 0f)
                {
                    newPhase = frog.Prototype.SwimKickDuration;
                }
                frog.SwimKickPhase = newPhase;
            }
            else if (frog.SwimDirection != Vector2.Zero)
            {
                if (frog.SwimKickPhase > 0f)
                {
                    frog.SwimKickPhase = Math.Max(0f, frog.SwimKickPhase - mDynamics.FixedTimeStep);
                    float accelerationModule = frog.Prototype.SwimKickVelocity * frog.Prototype.WaterDrag;
                    Vector2 acceleration = frog.SwimDirection;
                    acceleration.Normalize();
                    return accelerationModule * acceleration;
                }
            }
            else if (frog.SwimKickPhase != frog.Prototype.SwimKickDuration)
            {
                frog.SwimKickPhase = -frog.Prototype.SwimKickRecharge;
            }
            return Vector2.Zero;
        }

        private Vector2 ComputeBuouyancyAcceleration(FrogSimState frog)
        {
            Rect frogRect = new Rect(frog.Position, frog.Width, frog.Height);
            float volumePercentage = (mPondState.WaterLevel - frogRect.MinY) / (frog.Height);
            if (volumePercentage > 1f)
            {
                volumePercentage = 1f;
            }
            else if (volumePercentage < 0f)
            {
                volumePercentage = 0f;
            }
            return (frog.Prototype.FrogDensity - volumePercentage) * kGravity;
        }
    }
}
