using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Data;
using Ranitas.Pond;
using System;

namespace Ranitas.Sim
{
    public sealed class FlySpawnSystem : ISystem
    {
        public FlySpawnSystem(FrameTime time, PondSimState pond, FlyData flyData)
        {
            mPond = pond;
            mFlyData = flyData;
            mTime = time;
            mRandom = new Random();
        }

        private Random mRandom;
        private FlyData mFlyData;
        private PondSimState mPond;
        private FrameTime mTime;
        private FlyFactory mFactory;

        private float mTimeBeforeSpawn; //NOTE: System state

        private struct FliesSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<Rect> FlyRect;
            public SliceRequirement<Insect> IsInsect;
        }
        private FliesSlice mFliesSlice;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            mFactory = new FlyFactory(registry);
            registry.SetupSlice(ref mFliesSlice);
            mTimeBeforeSpawn = mRandom.NextPoissonTime(mFlyData.FliesPerSecond);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            const float kBuffer = 200;
            int flyCount = mFliesSlice.Entity.Count;
            for (int i = flyCount - 1; i >= 0; --i)
            {
                Rect flyRect = mFliesSlice.FlyRect[i]; 
                if ((flyRect.MaxX < -kBuffer) || (flyRect.MinX > mPond.Width + kBuffer))
                {
                    registry.Destroy(mFliesSlice.Entity[i]);
                    --flyCount;
                }
            }
            if (flyCount < mFlyData.MaxActiveFlies)
            {
                mTimeBeforeSpawn -= mTime.DeltaTime;
                if (mTimeBeforeSpawn <= 0f)
                {
                    SpawnFly(registry);
                    mTimeBeforeSpawn = mRandom.NextPoissonTime(mFlyData.FliesPerSecond);
                }
            }
        }

        private void SpawnFly(EntityRegistry registry)
        {
            float heightAboveWater = mRandom.GetRandomInRange(mFlyData.MinHeight, mFlyData.MaxHeight);
            float randomSpeed = mRandom.GetRandomInRange(mFlyData.MinSpeed, mFlyData.MaxSpeed);
            float flyHeight = mPond.WaterLevel + heightAboveWater;
            Vector2 flyInitialPosition;
            Vector2 flyVelocity = new Vector2(randomSpeed, 0);
            int coinFlip = mRandom.Next(0, 2);
            if (coinFlip == 1)
            {
                flyVelocity.X = -flyVelocity.X;
                flyInitialPosition = new Vector2(mPond.Width + 0.5f * mFlyData.Width, flyHeight);
            }
            else
            {
                flyInitialPosition = new Vector2(-0.5f * mFlyData.Width, flyHeight);
            }
            //TODO: Pass registry to factory!
            Entity fly = mFactory.MakeFly(flyInitialPosition, flyVelocity, mFlyData.Width, mFlyData.Height);
        }
    }
}
