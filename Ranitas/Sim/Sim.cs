using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Frog.Sim;
using Ranitas.Pond;
using System;
using System.Collections.Generic;
using static Ranitas.Frog.Sim.FrogSimState;

namespace Ranitas.Sim
{
    public sealed class RanitasSim
    {
        public readonly static Vector2 kGravity = new Vector2(0f, -980f);    //TODO: Data drive!

        private PondSimState mPondState;
        private List<FrogSimState> mFrogStates;

        private FixedTimeStepDynamics mDynamics;

        public RanitasSim(PondSimState pondState, List<FrogSimState> frogStates, float fixedTimeStep)
        {
            mPondState = pondState;
            mFrogStates = frogStates;
            mDynamics = new FixedTimeStepDynamics(fixedTimeStep);
        }

        public void Update()
        {
            UpdateFrogs();
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
                if (UpdateFrogJump(frog))
                {
                    ResetFrogJumpInput(frog);
                }
                else
                {
                    UpdateFrogJumpWarmup(frog);
                }
                UpdateDryFrogDynamics(frog.RigidBodyState);
                if (FrogCollidesWithLily(frog.RigidBodyState, out LilyPadSimState lily))
                {
                    OnFrogCollidesWithLily(frog, lily);
                }
                else
                {
                    UpdateNonGroundedFrogState(frog);
                }
            }
            else
            {
                ResetFrogJumpInput(frog);
                Vector2 swimAcceleration = UpdateFrogSwimAndGetAcceleration(frog);
                UpdateSwimingFrogDynamics(frog.RigidBodyState, swimAcceleration, frog.Prototype.WaterDrag);
                UpdateNonGroundedFrogState(frog);
            }
        }

        private void UpdateNonGroundedFrogState(FrogSimState frog)
        {
            if (frog.RigidBodyState.FeetPosition.Y < mPondState.WaterLevel)
            {
                if (frog.State != FrogState.Swimming)
                {
                    frog.State = FrogState.Swimming;
                    frog.SwimKickPhase = 0f;
                }
            }
            else
            {
                frog.State = FrogState.Airborne;
            }
        }

        private static bool UpdateFrogJump(FrogSimState frog)
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
                frog.RigidBodyState.Velocity += velocity;
                return true;
            }
            return false;
        }

        private static void ResetFrogJumpInput(FrogSimState frog)
        {
            frog.PreparingJump = false;
            frog.JumpSignaled = false;
            frog.TimePreparingJump = 0f;
        }

        private void UpdateFrogJumpWarmup(FrogSimState frog)
        {
            if (frog.PreparingJump)
            {
                float relativeSquish = frog.RelativeJumpStrength;
                float scale = relativeSquish * frog.Prototype.JumpSquish + (1f - relativeSquish);
                frog.RigidBodyState.Height = scale * frog.Prototype.Height;
                frog.RigidBodyState.Width = frog.Prototype.Width / scale;
            }
            else
            {
                frog.RigidBodyState.Height = frog.Prototype.Height;
                frog.RigidBodyState.Width = frog.Prototype.Width;
            }
        }

        private void UpdateDryFrogDynamics(RigidBodyState frogRigidBody)
        {
            frogRigidBody.Position += mDynamics.FramePositionDelta(kGravity, frogRigidBody.Velocity);
            frogRigidBody.Velocity = frogRigidBody.Velocity + mDynamics.FrameVelocityDelta(kGravity);
        }

        private bool FrogCollidesWithLily(RigidBodyState frogRigidBody, out LilyPadSimState hitLily)
        {
            if (frogRigidBody.Velocity.Y <= 0f)
            {
                foreach (var lily in mPondState.Lilies)
                {
                    Rect lilyRect = lily.Rect;
                    if (frogRigidBody.Rect.Intersects(lilyRect))
                    {
                        //Assuming only one collision is possible!
                        hitLily = lily;
                        return true;
                    }
                }
            }
            hitLily = null;
            return false;
        }

        private void OnFrogCollidesWithLily(FrogSimState frog, LilyPadSimState lily)
        {
            float landingY = lily.Rect.MaxY;
            Vector2 inheritedVelocity = lily.Velocity;
            frog.RigidBodyState.FeetPosition = new Vector2(frog.RigidBodyState.Position.X, landingY);
            frog.RigidBodyState.Velocity = inheritedVelocity;
            frog.State = FrogState.Grounded;
        }

        private void UpdateSwimingFrogDynamics(RigidBodyState frogRigidBody, Vector2 swimAcceleration, float waterDrag)
        {
            Vector2 buouyancy = ComputeBuouyancyAcceleration(frogRigidBody.Rect, frogRigidBody.Density);
            Vector2 totalAcceperation = buouyancy + swimAcceleration;
            if ((frogRigidBody.Velocity.Y > 0f) && (frogRigidBody.Position.Y + (0.5f * frogRigidBody.Height)) > mPondState.WaterLevel)
            {
                //No vertical drag!
                Vector2 verticalVelocity = new Vector2(0f, frogRigidBody.Velocity.Y);
                Vector2 verticalAcceleration = new Vector2(0f, totalAcceperation.Y);
                frogRigidBody.Position += mDynamics.FramePositionDelta(verticalAcceleration, verticalVelocity);
                Vector2 totalVerticalVelocity = verticalVelocity + mDynamics.FrameVelocityDelta(verticalAcceleration);

                Vector2 horizontalVelocity = new Vector2(frogRigidBody.Velocity.X, 0f);
                Vector2 horizontalAcceleration = new Vector2(totalAcceperation.X, 0f);
                UpdateFrogDragDynamics(frogRigidBody, horizontalVelocity, horizontalAcceleration, waterDrag);
                frogRigidBody.Velocity += totalVerticalVelocity;
            }
            else
            {
                UpdateFrogDragDynamics(frogRigidBody, frogRigidBody.Velocity, totalAcceperation, waterDrag);
            }
        }

        private void UpdateFrogDragDynamics(RigidBodyState frogRigidBody, Vector2 updateVelocity, Vector2 accelerationVector, float waterDrag)
        {
            frogRigidBody.Position += mDynamics.FrameLinearDragPositionDelta(updateVelocity, waterDrag, accelerationVector);
            frogRigidBody.Velocity = mDynamics.FrameLinearDragVelocity(updateVelocity, waterDrag, accelerationVector);
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

        private Vector2 ComputeBuouyancyAcceleration(Rect frogRect, float frogDensity)
        {
            float volumePercentage = (mPondState.WaterLevel - frogRect.MinY) / (frogRect.Height);
            if (volumePercentage > 1f)
            {
                volumePercentage = 1f;
            }
            else if (volumePercentage < 0f)
            {
                volumePercentage = 0f;
            }
            return (frogDensity - volumePercentage) * kGravity;
        }
    }
}
