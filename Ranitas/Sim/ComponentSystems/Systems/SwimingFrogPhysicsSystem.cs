using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Core.Physics;
using Ranitas.Pond;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class SwimingFrogPhysicsSystem : ISystem
    {
        public SwimingFrogPhysicsSystem(FrameTime frameTime, PondSimState pond, Data.FrogData frogData)
        {
            mTime = frameTime;
            mPond = pond;
            mSwimData = new FrogSwimData(frogData);
        }

        private FrameTime mTime;    //Currently a class, but should this be it's own copy?
        private PondSimState mPond;
        private FrogSwimData mSwimData;

        private struct WetFrogs
        {
            public SliceEntityOutput Entities;
            public SliceRequirementOutput<Waterborne> Waterborne;
            public SliceRequirementOutput<FrogControlState> Control;
            public SliceRequirementOutput<Position> Position;
            public SliceRequirementOutput<Rect> Rect;
            public SliceRequirementOutput<Velocity> Velocity;
        }
        private WetFrogs mWetFrogs = new WetFrogs();

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mWetFrogs);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
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

                float frogTop = mWetFrogs.Rect[i].MaxY;
                if ((velocity.Y > 0) && (frogTop > mPond.WaterLevel))
                {
                    //No vertical drag!
                    Vector2 verticalVelocity = new Vector2(0f, velocity.Y);
                    Vector2 verticalAcceleration = new Vector2(0f, totalAcceleration.Y);

                    //NOTE: BUG: If goping up, but swimming down, the acceleration is incorrectly applied(?)

                    positionDelta = Dynamics.NewtonianPositionDelta(verticalVelocity, verticalAcceleration,mTime);
                    velocityDelta = Dynamics.NewtonianVelocityDelta(verticalAcceleration, mTime);

                    velocity = new Vector2(velocity.X, 0f);
                    totalAcceleration = new Vector2(totalAcceleration.X, 0f);
                }

                float drag = mSwimData.WaterDrag;
                positionDelta += Dynamics.LinearDragPositionDelta(velocity, drag, totalAcceleration, mTime);
                velocityDelta += Dynamics.LinearDragVelocityDelta(velocity, drag, totalAcceleration, mTime);

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
            Rect rect = mWetFrogs.Rect[iterationIndex];
            float density = mSwimData.Density;
            return ComputeBuouyancyAcceleration(rect, density, mPond);
        }

        private Vector2 UpdateFrogSwimAndGetAcceleration(EntityRegistry registry, int iterationIndex)
        {
            Vector2 swimAcceleration = Vector2.Zero;
            if (mWetFrogs.Waterborne[iterationIndex].SwimKickPhase > 0f)
            {
                swimAcceleration = mSwimData.SwimAccelerationModule * mWetFrogs.Control[iterationIndex].InputDirection;
            }
            return swimAcceleration;
        }

        private static Vector2 ComputeBuouyancyAcceleration(Rect frogRect, float frogDensity, PondSimState pondState)
        {
            float volumePercentage = (pondState.WaterLevel - frogRect.MinY) / (frogRect.Height);
            volumePercentage = MathExtensions.Clamp01(volumePercentage);
            return (frogDensity - volumePercentage) * PondSimState.kGravity;
        }
    }
}
