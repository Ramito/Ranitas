using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Pond;
using System;
using System.Diagnostics;

namespace Ranitas.Sim
{
    public sealed class FrogPhysicsSystem : ISystem
    {
        public FrogPhysicsSystem(FrameTime frameTime, PondSimState pond, Data.FrogData frogData)
        {
            mTime = frameTime;
            mPond = pond;
            mSwimData = new FrogSwimData(frogData);
        }

        private FrameTime mTime;    //Currently a class, but should this be it's own copy?
        private PondSimState mPond;
        private FrogSwimData mSwimData;

        private struct DryFrogs
        {
            public SliceEntityOutput Entities;
            public SliceExclusion<Waterborne> NotWet;
            public SliceRequirementOutput<Position> Positions;
            public SliceRequirementOutput<Velocity> Velocities;
        }
        private DryFrogs mDryFrogs = new DryFrogs();

        //TODO: Separate wet and dry systems to simplify this! There is also risk of confusing slices!
        private struct WetFrogs
        {
            public SliceEntityOutput Entities;
            public SliceRequirementOutput<Waterborne> Waterborne;
            public SliceRequirementOutput<FrogControlState> Control;
            public SliceRequirementOutput<Position> Position;
            public SliceRequirementOutput<RectShape> RectShape;
            public SliceRequirementOutput<Velocity> Velocity;
        }
        private WetFrogs mWetFrogs = new WetFrogs();

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mDryFrogs);
            registry.SetupSlice(ref mWetFrogs);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            //TODO: Frog shapes
            UpdateAirborneFrogs(registry);
            UpdateWaterborneFrogs(registry);
        }

        private void UpdateAirborneFrogs(EntityRegistry registry)
        {
            int count = mDryFrogs.Entities.Count;
            for (int i = 0; i < count; ++i)
            {
                Vector2 velocityDelta = FrameVelocityDelta(PondSimState.kGravity);
                Vector2 newVelocity = mDryFrogs.Velocities[i].Value + velocityDelta;
                Vector2 frogPositionDelta = FramePositionDelta(mDryFrogs.Velocities[i].Value, PondSimState.kGravity);
                Vector2 newPosition = mDryFrogs.Positions[i].Value + frogPositionDelta;
                //TODO: Deferred registry commands?
                registry.SetComponent(mDryFrogs.Entities[i], new Velocity(newVelocity));
                registry.SetComponent(mDryFrogs.Entities[i], new Position(newPosition));
            }

        }

        private Vector2 FramePositionDelta(Vector2 frameVelocity, Vector2 acceleration)
        {
            Vector2 velocityContribution = mTime.DeltaTime * frameVelocity;
            Vector2 accelerationContribution = mTime.HalfDeltaSquaredTime * acceleration;
            return velocityContribution + accelerationContribution;
        }

        private Vector2 FrameVelocityDelta(Vector2 acceleration)
        {
            return mTime.DeltaTime * acceleration;
        }

        public Vector2 FrameLinearDragVelocityDelta(Vector2 frameVelocity, float dragCoefficient, Vector2 acceleration)
        {
            Debug.Assert(dragCoefficient > 0f); //TODO: Enforce even if data is bad!
            float dragFactor = (float)Math.Exp(-dragCoefficient * mTime.DeltaTime);
            float accelerationModule = acceleration.Length();
            Vector2 terminalVelocity = (1f / dragCoefficient) * acceleration;
            return (dragFactor - 1f) * (frameVelocity - terminalVelocity);
        }

        public Vector2 FrameLinearDragPositionDelta(Vector2 frameVelocity, float dragCoefficient, Vector2 acceleration)
        {
            Debug.Assert(dragCoefficient > 0f); //TODO: Enforce even if data is bad!
            float dragFactor = (float)Math.Exp(-dragCoefficient * mTime.DeltaTime);
            float accelerationModule = acceleration.Length();
            Vector2 terminalVelocity = (1f / dragCoefficient) * acceleration;
            return (terminalVelocity * mTime.DeltaTime) + ((frameVelocity - terminalVelocity) / dragCoefficient) * (1f - dragFactor);
        }

        private void UpdateWaterborneFrogs(EntityRegistry registry)
        {
            int count = mWetFrogs.Entities.Count;
            for (int i = 0; i < count; ++i)
            {
                Vector2 swimAcceleration = UpdateFrogSwimAndGetAcceleration(registry, i);
                Vector2 bouyancyAcceleration = GetBouyancyAcceleration(i);
                Vector2 totalAcceleration = swimAcceleration + bouyancyAcceleration;

                Vector2 originalPosition = mWetFrogs.Position[i].Value;
                Vector2 position = originalPosition;
                Vector2 originalVelocity = mWetFrogs.Velocity[i].Value;
                Vector2 velocity = originalVelocity;

                Vector2 positionDelta = Vector2.Zero;
                Vector2 velocityDelta = Vector2.Zero;

                Vector2 headPosition = position + (0.5f * mWetFrogs.RectShape[i].Height) * Vector2.UnitY;
                if ((velocity.Y > 0) && (headPosition.Y > mPond.WaterLevel))
                {
                    //No vertical drag!
                    Vector2 verticalVelocity = new Vector2(0f, velocity.Y);
                    Vector2 verticalAcceleration = new Vector2(0f, totalAcceleration.Y);

                    //NOTE: BUG: If goping up, but swimming down, the acceleration is incorrectly applied(?)

                    positionDelta = FramePositionDelta(verticalVelocity, verticalAcceleration);
                    velocityDelta = FrameVelocityDelta(verticalAcceleration);

                    velocity = new Vector2(velocity.X, 0f);
                    totalAcceleration = new Vector2(totalAcceleration.X, 0f);
                    //BUG: Vertical deltas lost!
                }

                float drag = mSwimData.WaterDrag;
                positionDelta += FrameLinearDragPositionDelta(velocity, drag, totalAcceleration);
                velocityDelta += FrameLinearDragVelocityDelta(velocity, drag, totalAcceleration);

                position = originalPosition + positionDelta;
                velocity = originalVelocity + velocityDelta;

                //TODO: Deferred registry commands?
                registry.SetComponent(mWetFrogs.Entities[i], new Position(position));
                registry.SetComponent(mWetFrogs.Entities[i], new Velocity(velocity));
            }
        }

        private Vector2 GetBouyancyAcceleration(int iterationIndex)
        {
            Position position = mWetFrogs.Position[iterationIndex];
            RectShape rectShape = mWetFrogs.RectShape[iterationIndex];
            Rect rect = CommonFrogProperties.FrogRect(position, rectShape); //TODO: System to condense these two into a single coponent?
            float density = mSwimData.Density;
            return CommonFrogProperties.ComputeBuouyancyAcceleration(rect, density, mPond);
        }

        private Vector2 UpdateFrogSwimAndGetAcceleration(EntityRegistry registry, int iterationIndex)
        {
            Vector2 swimAcceleration = Vector2.Zero;
            float swimKickPhase = mWetFrogs.Waterborne[iterationIndex].SwimKickPhase;
            if ((swimKickPhase > 0f) && (mWetFrogs.Control[iterationIndex].InputDirection != Vector2.Zero))
            {
                swimAcceleration = mWetFrogs.Control[iterationIndex].InputDirection;
                swimAcceleration.Normalize();
                swimAcceleration = mSwimData.SwimAccelerationModule * swimAcceleration;
            }
            return swimAcceleration;
        }
    }
}
