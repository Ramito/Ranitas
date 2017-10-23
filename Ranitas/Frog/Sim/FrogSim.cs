using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Input;
using Ranitas.Pond;
using System;
using System.Collections.Generic;
using static Ranitas.Frog.Sim.FrogSimState;

namespace Ranitas.Frog.Sim
{
    public static class FrogSim
    {
        public static void UpdateFrogInputs(FrogSimState frog, FrogInput input, float deltaTime)
        {
            if (frog.GameState.State != FrogGameState.States.Swimming)
            {
                float jumpInputMagnitude = 0f;
                if (frog.GameState.State == FrogGameState.States.Grounded)
                {
                    if (input.NormalizedDirection.Y >= 0.7f)
                    {
                        jumpInputMagnitude = input.Magnitude;
                    }
                }
                    float newJumpPercentage = frog.GameState.JumpPercentage + deltaTime / (frog.Prototype.JumpPrepareTime);
                    frog.GameState.JumpPercentage = MathHelper.Clamp(newJumpPercentage, 0f, jumpInputMagnitude);
                    if (jumpInputMagnitude != 0)
                    {
                        if ((input.Signals & FrogSignals.Jump) != 0)
                        {
                            frog.RigidBodyState.Velocity += ((frog.GameState.JumpPercentage * frog.Prototype.JumpVelocity) * input.NormalizedDirection);
                            frog.GameState.JumpPercentage = 0f;
                        }
                    }
                frog.Toungue.ExtendSignal = ((input.Signals & FrogSignals.Toungue) != 0);
            }
            else
            {
                frog.Toungue.ExtendSignal = false;
                frog.GameState.JumpPercentage = 0f;
            }
            frog.GameState.InputDirection = input.NormalizedDirection;
            if (!frog.Toungue.ToungueActive && (frog.Toungue.RelativeLength == 0f))
            {
                if (input.NormalizedDirection.X > 0f)
                {
                    frog.ToungueDirection = 1;
                }
                else if (input.NormalizedDirection.X < 0f)
                {
                    frog.ToungueDirection = -1;
                }
            }
        }

        public static void UpdateFrogs(List<FrogSimState> frogStates, PondSimState pondState, FixedTimeStepDynamics dynamics)
        {
            foreach (var frog in frogStates)
            {
                UpdateFrog(frog, pondState, dynamics);
                UpdateFrogToungue(frog, dynamics.FixedTimeStep);
            }
        }

        private static void UpdateFrog(FrogSimState frog, PondSimState pondState, FixedTimeStepDynamics dynamics)
        {
            if (frog.GameState.State != FrogGameState.States.Swimming)
            {
                UpdateFrogShape(frog);
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
                Vector2 swimAcceleration = UpdateFrogSwimAndGetAcceleration(frog, dynamics);
                UpdateSwimingFrogDynamics(frog.RigidBodyState, swimAcceleration, frog.Prototype.WaterDrag, pondState, dynamics);
                UpdateNonGroundedFrogState(frog, pondState);
            }
        }

        private static void UpdateFrogToungue(FrogSimState frog, float deltaTime)
        {
            frog.Toungue.Update(deltaTime);
        }

        private static void UpdateNonGroundedFrogState(FrogSimState frog, PondSimState pondState)
        {
            if (frog.RigidBodyState.FeetPosition.Y < pondState.WaterLevel)
            {
                if (frog.GameState.State != FrogGameState.States.Swimming)
                {
                    frog.GameState.State = FrogGameState.States.Swimming;
                    frog.SwimKickPhase = 0f;
                }
            }
            else
            {
                frog.GameState.State = FrogGameState.States.Airborne;
            }
        }

        private static void UpdateFrogShape(FrogSimState frog)
        {
            float relativeSquish = frog.GameState.JumpPercentage;
            float scale = relativeSquish * frog.Prototype.JumpSquish + (1f - relativeSquish);
            frog.RigidBodyState.Height = scale * frog.Prototype.Height;
            frog.RigidBodyState.Width = frog.Prototype.Width / scale;
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
            frog.GameState.State = FrogGameState.States.Grounded;
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
            else if (frog.GameState.InputDirection != Vector2.Zero)
            {
                if (frog.SwimKickPhase > 0f)
                {
                    frog.SwimKickPhase = Math.Max(0f, frog.SwimKickPhase - dynamics.FixedTimeStep);
                    float accelerationModule = frog.Prototype.SwimKickVelocity * frog.Prototype.WaterDrag;
                    Vector2 acceleration = frog.GameState.InputDirection;
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
