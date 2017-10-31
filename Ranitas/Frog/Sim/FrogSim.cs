using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.EventSystem;
using Ranitas.Input;
using Ranitas.Pond;
using System;
using System.Collections.Generic;

namespace Ranitas.Frog.Sim
{
    public class FrogSim
    {
        private readonly PlayerBinding[] mPlayerBindings;
        public readonly List<FrogSimState> FrogStates;

        public readonly FrogControlUpdater ControlUpdater;

        public FrogSim(float deltaTime, EventSystem eventSystem, PlayerBinding[] playerBindings, List<FrogSimState> frogStates)
        {
            mPlayerBindings = playerBindings;
            ControlUpdater = new FrogControlUpdater(deltaTime, eventSystem);
            eventSystem.AddMessageReceiver<JumpMessage>(OnFrogJump);
            eventSystem.AddMessageReceiver<ToungueMessage>(OnFrogToungue);
            FrogStates = frogStates;
        }

        private void OnFrogJump(JumpMessage jump)
        {
            mPlayerBindings[jump.ID].Frog.RigidBodyState.Velocity += jump.Velocity;
        }

        private void OnFrogToungue(ToungueMessage toungue)
        {
            mPlayerBindings[toungue.ID].Frog.Toungue.ExtendSignal = true;
            mPlayerBindings[toungue.ID].Frog.ToungueDirection = toungue.Direction;
        }

        public void Update(FrogInput[] inputs)
        {
            foreach (var binding in mPlayerBindings)
            {
                if (binding != null)
                {
                    ControlUpdater.Update(binding.PlayerIndex, binding.Frog.Prototype.MovementData, binding.Frog.ControlState, inputs[binding.PlayerIndex]);
                }
            }
        }

        public void UpdateFrogs(PondSimState pondState, FixedTimeStepDynamics dynamics)
        {
            foreach (var frog in FrogStates)
            {
                UpdateFrog(frog, pondState, dynamics);
                UpdateFrogToungue(frog, dynamics.FixedTimeStep);
            }
        }

        private static void UpdateFrog(FrogSimState frog, PondSimState pondState, FixedTimeStepDynamics dynamics)
        {
            if (frog.ControlState.State != FrogControlState.States.Swimming)
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
                if (frog.ControlState.State != FrogControlState.States.Swimming)
                {
                    frog.ControlState.State = FrogControlState.States.Swimming;
                    frog.SwimKickPhase = 0f;
                }
            }
            else
            {
                frog.ControlState.State = FrogControlState.States.Airborne;
            }
        }

        private static void UpdateFrogShape(FrogSimState frog)
        {
            float relativeSquish = frog.ControlState.RelativeJumpPower;
            float scale = relativeSquish * frog.Prototype.MovementData.JumpSquish + (1f - relativeSquish);
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
            frog.ControlState.State = FrogControlState.States.Grounded;
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
            else if (frog.ControlState.InputDirection != Vector2.Zero)
            {
                if (frog.SwimKickPhase > 0f)
                {
                    frog.SwimKickPhase = Math.Max(0f, frog.SwimKickPhase - dynamics.FixedTimeStep);
                    float accelerationModule = frog.Prototype.SwimKickVelocity * frog.Prototype.WaterDrag;
                    Vector2 acceleration = frog.ControlState.InputDirection;
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
