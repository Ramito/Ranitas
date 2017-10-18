using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Pond;
using System;
using System.Collections.Generic;
using static Ranitas.Frog.Sim.FrogSimState;

namespace Ranitas.Frog.Sim
{
    public static class FrogSim
    {
        public static void UpdateFrogs(List<FrogSimState> frogStates, PondSimState pondState, FixedTimeStepDynamics dynamics)
        {
            foreach (var frog in frogStates)
            {
                UpdateFrog(frog, pondState, dynamics);
                UpdateFrogToungue(frog);
            }
        }

        private static void UpdateFrog(FrogSimState frog, PondSimState pondState, FixedTimeStepDynamics dynamics)
        {
            if (frog.PreparingJump)
            {
                frog.TimePreparingJump += dynamics.FixedTimeStep;
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
                UpdateDryFrogDynamics(frog.RigidBodyState, dynamics);
                if (FrogCollidesWithLily(frog.RigidBodyState, pondState, out LilyPadSimState lily))
                {
                    OnFrogCollidesWithLily(frog, lily);
                }
                else
                {
                    UpdateNonGroundedFrogState(frog, pondState);
                }
            }
            else
            {
                ResetFrogJumpInput(frog);
                Vector2 swimAcceleration = UpdateFrogSwimAndGetAcceleration(frog, dynamics);
                UpdateSwimingFrogDynamics(frog.RigidBodyState, swimAcceleration, frog.Prototype.WaterDrag, pondState, dynamics);
                UpdateNonGroundedFrogState(frog, pondState);
            }
        }

        private static void UpdateFrogToungue(FrogSimState frog)
        {
            frog.Toungue.Update();
        }

        private static void UpdateNonGroundedFrogState(FrogSimState frog, PondSimState pondState)
        {
            if (frog.RigidBodyState.FeetPosition.Y < pondState.WaterLevel)
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

        private static void UpdateFrogJumpWarmup(FrogSimState frog)
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

        private static void UpdateDryFrogDynamics(RigidBodyState frogRigidBody, FixedTimeStepDynamics dynamics)
        {
            frogRigidBody.Position += dynamics.FramePositionDelta(PondSimState.kGravity, frogRigidBody.Velocity);
            frogRigidBody.Velocity = frogRigidBody.Velocity + dynamics.FrameVelocityDelta(PondSimState.kGravity);
        }

        private static bool FrogCollidesWithLily(RigidBodyState frogRigidBody, PondSimState pondState, out LilyPadSimState hitLily)
        {
            if (frogRigidBody.Velocity.Y <= 0f)
            {
                foreach (var lily in pondState.Lilies)
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

        private static void OnFrogCollidesWithLily(FrogSimState frog, LilyPadSimState lily)
        {
            float landingY = lily.Rect.MaxY;
            Vector2 inheritedVelocity = lily.Velocity;
            frog.RigidBodyState.FeetPosition = new Vector2(frog.RigidBodyState.Position.X, landingY);
            frog.RigidBodyState.Velocity = inheritedVelocity;
            frog.State = FrogState.Grounded;
        }

        private static void UpdateSwimingFrogDynamics(RigidBodyState frogRigidBody, Vector2 swimAcceleration, float waterDrag, PondSimState pondState, FixedTimeStepDynamics dynamics)
        {
            Vector2 buouyancy = ComputeBuouyancyAcceleration(frogRigidBody.Rect, frogRigidBody.Density, pondState);
            Vector2 totalAcceperation = buouyancy + swimAcceleration;
            if ((frogRigidBody.Velocity.Y > 0f) && (frogRigidBody.Position.Y + (0.5f * frogRigidBody.Height)) > pondState.WaterLevel)
            {
                //No vertical drag!
                Vector2 verticalVelocity = new Vector2(0f, frogRigidBody.Velocity.Y);
                Vector2 verticalAcceleration = new Vector2(0f, totalAcceperation.Y);
                frogRigidBody.Position += dynamics.FramePositionDelta(verticalAcceleration, verticalVelocity);
                Vector2 totalVerticalVelocity = verticalVelocity + dynamics.FrameVelocityDelta(verticalAcceleration);

                Vector2 horizontalVelocity = new Vector2(frogRigidBody.Velocity.X, 0f);
                Vector2 horizontalAcceleration = new Vector2(totalAcceperation.X, 0f);
                UpdateFrogDragDynamics(frogRigidBody, horizontalVelocity, horizontalAcceleration, waterDrag, dynamics);
                frogRigidBody.Velocity += totalVerticalVelocity;
            }
            else
            {
                UpdateFrogDragDynamics(frogRigidBody, frogRigidBody.Velocity, totalAcceperation, waterDrag, dynamics);
            }
        }

        private static void UpdateFrogDragDynamics(RigidBodyState frogRigidBody, Vector2 updateVelocity, Vector2 accelerationVector, float waterDrag, FixedTimeStepDynamics dynamics)
        {
            frogRigidBody.Position += dynamics.FrameLinearDragPositionDelta(updateVelocity, waterDrag, accelerationVector);
            frogRigidBody.Velocity = dynamics.FrameLinearDragVelocity(updateVelocity, waterDrag, accelerationVector);
        }

        private static Vector2 UpdateFrogSwimAndGetAcceleration(FrogSimState frog, FixedTimeStepDynamics dynamics)
        {
            if (frog.SwimKickPhase < 0f)
            {
                float newPhase = frog.SwimKickPhase + dynamics.FixedTimeStep;
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
                    frog.SwimKickPhase = Math.Max(0f, frog.SwimKickPhase - dynamics.FixedTimeStep);
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

        private static Vector2 ComputeBuouyancyAcceleration(Rect frogRect, float frogDensity, PondSimState pondState)
        {
            float volumePercentage = (pondState.WaterLevel - frogRect.MinY) / (frogRect.Height);
            if (volumePercentage > 1f)
            {
                volumePercentage = 1f;
            }
            else if (volumePercentage < 0f)
            {
                volumePercentage = 0f;
            }
            return (frogDensity - volumePercentage) * PondSimState.kGravity;
        }
    }
}
