using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Data;
using Ranitas.Pond;
using System;

namespace Ranitas.Sim
{
    public sealed class FlyDirectionSystem : ISystem
    {
        public FlyDirectionSystem(FrameTime time, PondSimState pond, FlyData flyData)
        {
            mTime = time;
            mPond = pond;
            mFlyData = flyData;
            mRandom = new Random();
        }

        private FlyData mFlyData;
        private PondSimState mPond;
        private FrameTime mTime;
        private Random mRandom;

        private struct WaitingToChange
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<Position> Position;
            public SliceRequirementOutput<Velocity> Velocity;
            public SliceRequirementOutput<ChangeDirectionTimer> ChangeTimer;
            public SliceRequirement<Insect> IsInsect;
        }
        private WaitingToChange mWaitingToChange;

        private struct Changing
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<Position> Position;
            public SliceRequirementOutput<Velocity> Velocity;
            public SliceExclusion<ChangeDirectionTimer> NoChangeTimer;
            public SliceRequirement<Insect> IsInsect;
        }
        private Changing mChanging;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mWaitingToChange);
            registry.SetupSlice(ref mChanging);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int waitingCount = mWaitingToChange.Entity.Count;
            for (int i = waitingCount - 1; i >= 0; --i)
            {
                bool forceChange = (mWaitingToChange.Position[i].Value.Y < (mFlyData.MinHeight + mPond.WaterLevel)) && (mWaitingToChange.Velocity[i].Value.Y < 0f);
                forceChange = forceChange || ((mWaitingToChange.Position[i].Value.Y > (mFlyData.MaxHeight + mPond.WaterLevel)) && (mWaitingToChange.Velocity[i].Value.Y > 0f));
                if (forceChange)
                {
                    registry.RemoveComponent<ChangeDirectionTimer>(mWaitingToChange.Entity[i]);
                }
            }
            waitingCount = mWaitingToChange.Entity.Count;
            for (int i = waitingCount - 1; i >= 0; --i)
            {
                float time = mWaitingToChange.ChangeTimer[i].TimeLeft - mTime.DeltaTime;
                if (time <= 0f)
                {
                    registry.RemoveComponent<ChangeDirectionTimer>(mWaitingToChange.Entity[i]);
                }
                else
                {
                    registry.SetComponent(mWaitingToChange.Entity[i], new ChangeDirectionTimer(time));
                }
            }
            int changingCount = mChanging.Entity.Count;
            for (int i = 0; i < changingCount; ++i)
            {
                float heightAboveWater = mChanging.Position[i].Value.Y - mPond.WaterLevel;
                Vector2 velocity = mChanging.Velocity[i].Value;
                Vector2 newVelocity = FlyDirection(heightAboveWater, velocity);
                registry.SetComponent(mChanging.Entity[i], new Velocity(newVelocity));
            }
            for (int i = changingCount - 1; i >= 0; --i)
            {
                const float rateOfChange = 2.75f;
                registry.AddComponent(mChanging.Entity[i], new ChangeDirectionTimer(mRandom.NextPoissonTime(rateOfChange)));
            }
        }

        private Vector2 FlyDirection(float heightAboveWater, Vector2 currentVelocity)
        {
            float angleToRotate = ComputeVelocityRotation(heightAboveWater);
            if (currentVelocity.X < 0f)
            {
                angleToRotate = -angleToRotate;
            }
            float cos = (float)Math.Cos(angleToRotate);
            float sin = (float)Math.Sin(angleToRotate);
            float newX = cos * currentVelocity.X - sin * currentVelocity.Y;
            float newY = sin * currentVelocity.X + cos * currentVelocity.Y;
            return new Vector2(newX, newY);
        }

        private float ComputeVelocityRotation(float heightAboveWater)
        {
            const float halfRange = (float)Math.PI / 17f;
            float relativeHeight = MathExtensions.Clamp01((heightAboveWater - mFlyData.MinHeight) / (mFlyData.MaxHeight - mFlyData.MinHeight));
            float rangeOffset = halfRange * (-2f * relativeHeight + 1f);
            return rangeOffset + mRandom.GetRandomInRange(-halfRange, halfRange);
        }
    }
}
