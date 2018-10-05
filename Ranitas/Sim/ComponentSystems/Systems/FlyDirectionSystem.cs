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
                float speed = velocity.Length();
                Vector2 newVelocity = speed * FlyDirection(heightAboveWater);
                if (Math.Sign(velocity.X) < 0)
                {
                    newVelocity.X = -newVelocity.X;
                }
                registry.SetComponent(mChanging.Entity[i], new Velocity(newVelocity));
            }
            for (int i = changingCount - 1; i >= 0; --i)
            {
                const float rateOfChange = 1.5f;
                registry.AddComponent(mChanging.Entity[i], new ChangeDirectionTimer(mRandom.NextPoissonTime(rateOfChange)));
            }
        }

        private Vector2 FlyDirection(float heightAboveWater)
        {
            const float bottomAngle = (float)Math.PI / 3f;
            const float topAngle = -bottomAngle;
            const float halfRange = (float)Math.PI / 3f;
            float relativeHeight = MathExtensions.Clamp01((heightAboveWater - mFlyData.MinHeight) / (mFlyData.MaxHeight - mFlyData.MinHeight));
            float resultingAngle = relativeHeight * (topAngle - bottomAngle) + bottomAngle + mRandom.GetRandomInRange(-halfRange, halfRange);
            return new Vector2((float)Math.Cos(resultingAngle), (float)Math.Sin(resultingAngle));
        }
    }
}
