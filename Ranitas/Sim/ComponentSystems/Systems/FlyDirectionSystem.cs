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
        public FlyDirectionSystem(FrameTime time, PondSimState pond, FlyData flyData, FlyDirectionChangeData changeData)
        {
            mTime = time;
            mPond = pond;
            mFlyData = flyData;
            mChangeData = changeData;
            mRandom = new Random();
        }

        private FlyData mFlyData;
        private FlyDirectionChangeData mChangeData;
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
            CheckOutOfBoundsFlies(registry);
            CheckChangeTimers(registry);
            DoDirectionChanges(registry);
        }

        private void CheckOutOfBoundsFlies(EntityRegistry registry)
        {
            int waitingCount = mWaitingToChange.Entity.Count;
            for (int i = waitingCount - 1; i >= 0; --i)
            {
                bool flyingLow = mWaitingToChange.Position[i].Value.Y < (mFlyData.MinHeight + mPond.WaterLevel);
                bool forceChange = flyingLow || (mWaitingToChange.Position[i].Value.Y > (mFlyData.MaxHeight + mPond.WaterLevel));
                if (forceChange)
                {
                    Vector2 velocity = mWaitingToChange.Velocity[i].Value;
                    float angle = mChangeData.TurnAroundRate * mTime.DeltaTime;
                    if ((velocity.X < 0) && flyingLow)
                    {
                        angle = -angle;
                    }
                    else if ((velocity.X > 0) && !flyingLow)
                    {
                        angle = -angle;
                    }
                    Vector2 newVelocity = MathExtensions.Rotate(velocity, angle);
                    registry.SetComponent(mWaitingToChange.Entity[i], new Velocity(newVelocity));
                }
            }
        }

        private void CheckChangeTimers(EntityRegistry registry)
        {
            int waitingCount = mWaitingToChange.Entity.Count;
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
        }

        private void DoDirectionChanges(EntityRegistry registry)
        {
            int changingCount = mChanging.Entity.Count;
            for (int i = 0; i < changingCount; ++i)
            {
                float heightAboveWater = mChanging.Position[i].Value.Y - mPond.WaterLevel;
                Vector2 velocity = mChanging.Velocity[i].Value;
                Vector2 newVelocity = NewFlyVelocity(heightAboveWater, velocity);
                registry.SetComponent(mChanging.Entity[i], new Velocity(newVelocity));
            }
            for (int i = changingCount - 1; i >= 0; --i)
            {
                registry.AddComponent(mChanging.Entity[i], new ChangeDirectionTimer(mRandom.NextPoissonTime(mChangeData.ChangeRate)));
            }
        }

        private Vector2 NewFlyVelocity(float heightAboveWater, Vector2 currentVelocity)
        {
            float angleToRotate = ComputeVelocityRotation(heightAboveWater);
            if (currentVelocity.X < 0f)
            {
                angleToRotate = -angleToRotate;
            }
            return MathExtensions.Rotate(currentVelocity, angleToRotate);
        }

        private float ComputeVelocityRotation(float heightAboveWater)
        {
            float relativeHeight = MathExtensions.Clamp01((heightAboveWater - mFlyData.MinHeight) / (mFlyData.MaxHeight - mFlyData.MinHeight));
            float halfRange = mChangeData.MaxDelta;
            float rangeOffset = halfRange * (-2f * relativeHeight + 1f);
            return rangeOffset + mRandom.GetRandomInRange(-halfRange, halfRange);
        }
    }
}
