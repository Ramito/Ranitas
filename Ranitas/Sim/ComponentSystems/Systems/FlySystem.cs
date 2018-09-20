using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Data;
using Ranitas.Pond;
using System;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class FlySystem : ISystem
    {
        public FlySystem(FrameTime time, PondSimState pond, FlyData flyData)
        {
            mPond = pond;
            mFlyData = flyData;
            mTime = time;
            mRandom = new Random();
            mTimeBeforeSpawn = 1f / mFlyData.FliesPerSecond;
            mPositionCache = new List<Vector2>(mFlyData.MaxActiveFlies);
        }

        private FlyFactory mFactory;
        private PondSimState mPond;
        private FlyData mFlyData;
        private FrameTime mTime;
        private Random mRandom;
        private float mTimeBeforeSpawn; //TODO: System state! Alternative?
        private List<Vector2> mPositionCache;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            mFactory = new FlyFactory(registry);
            registry.SetupSlice(ref mFliesSlice);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int flyCount = mFliesSlice.Entity.Count;
            if (flyCount < mFlyData.MaxActiveFlies)
            {
                mTimeBeforeSpawn -= mTime.DeltaTime;
                if (mTimeBeforeSpawn <= 0f)
                {
                    SpawnFly();
                    mTimeBeforeSpawn = 1f / mFlyData.FliesPerSecond;
                }
            }
            flyCount = mFliesSlice.Entity.Count;
            for (int i = 0; i < flyCount; ++i)
            {
                Vector2 newPosition = mFliesSlice.Position[i].Value + mTime.DeltaTime * mFliesSlice.Velocity[i].Value;
                mPositionCache.Add(newPosition);
            }
            for (int i = 0; i < flyCount; ++i)
            {
                registry.SetComponent(mFliesSlice.Entity[i], new Position(mPositionCache[i]));
            }
            for (int i = flyCount - 1; i >= 0; --i)
            {
                float x = mPositionCache[i].X;
                if ((x < -mFlyData.Width) || (x > (mPond.Width + mFlyData.Width)))
                {
                    registry.Destroy(mFliesSlice.Entity[i]);    //TODO: Post-Update commands!
                }
            }
            mPositionCache.Clear();
        }

        private void SpawnFly()
        {
            float heightAboveWater = GetRandomInRange(mFlyData.MinHeight, mFlyData.MaxHeight);
            float randomSpeed = GetRandomInRange(mFlyData.MinSpeed, mFlyData.MaxSpeed);
            float flyHeight = mPond.WaterLevel + heightAboveWater;
            float flySpeed = randomSpeed;
            Vector2 flyInitialPosition;
            int coinFlip = mRandom.Next(0, 2);
            if (coinFlip == 1)
            {
                flySpeed = -flySpeed;
                flyInitialPosition = new Vector2(mPond.Width + 0.5f * mFlyData.Width, flyHeight);
            }
            else
            {
                flyInitialPosition = new Vector2(-0.5f * mFlyData.Width, flyHeight);
            }
            Vector2 velocity = new Vector2(flySpeed, 0f);
            mFactory.MakeFly(flyInitialPosition, velocity, mFlyData.Width, mFlyData.Height);
        }

        private float GetRandomInRange(float min, float max)
        {
            return min + (float)mRandom.NextDouble() * (max - min);
        }

        private struct FliesSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<Position> Position;
            public SliceRequirementOutput<Velocity> Velocity;
            public SliceRequirement<Insect> IsInsect;
        }
        private FliesSlice mFliesSlice;
    }
}
