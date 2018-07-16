using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Pond;
using Ranitas.Data;
using System;
using System.Collections.Generic;

namespace Ranitas.Insects
{
    public sealed class FlySim
    {
        public readonly FlyData FlyData;

        private PondSimState mPond;
        private FixedTimeStepDynamics mDynamics;
        public List<FlySimState> ActiveFlies; //TODO: Formalize if we are using public data pattern or other!
        private List<FlySimState> mFlyPool;
        private Random mRandom;

        private float mFlyTickSpawnProbability;

        public FlySim(FlyData flyData, PondSimState pond, FixedTimeStepDynamics dynamics)
        {
            FlyData = flyData;
            mPond = pond;
            mDynamics = dynamics;
            ActiveFlies = new List<FlySimState>(flyData.MaxActiveFlies);
            mFlyPool = new List<FlySimState>(flyData.MaxActiveFlies);
            for (int i = 0; i < flyData.MaxActiveFlies; ++i)
            {
                mFlyPool.Add(new FlySimState());
            }
            mFlyTickSpawnProbability = (float)Math.Exp(-FlyData.FliesPerSecond * mDynamics.FixedTimeStep);
            mRandom = new Random();
        }

        public void Update()
        {
            if (ShouldSpawnFly())
            {
                SpawnFly();
            }
            Rect pondRect = new Rect(Vector2.Zero, new Vector2(mPond.Width, mPond.Height));
            for (int i = ActiveFlies.Count - 1; i >= 0; --i)
            {
                FlySimState currentFly = ActiveFlies[i];
                Vector2 newPosition = currentFly.Position + mDynamics.FramePositionDelta(Vector2.Zero, currentFly.Speed * Vector2.UnitX);
                Rect flyRect = new Rect(newPosition, FlyData.Width, FlyData.Height);
                if (!pondRect.Intersects(flyRect))
                {
                    DespawnFly(i);
                    continue;
                }
                currentFly.Position = newPosition;
            }
        }

        public void DespawnFly(int flyIndex)
        {
            //TODO: Can we not expose this?
            FlySimState flyToRemove = ActiveFlies[flyIndex];
            ActiveFlies.RemoveAt(flyIndex);
            mFlyPool.Add(flyToRemove);
        }

        private void SpawnFly()
        {

            float heightAboveWater = GetRandomInRange(FlyData.MinHeight, FlyData.MaxHeight);
            float randomSpeed = GetRandomInRange(FlyData.MinSpeed, FlyData.MaxSpeed);
            float flyHeight = mPond.WaterLevel + heightAboveWater;
            float flySpeed = randomSpeed;
            Vector2 flyInitialPosition;
            int coinFlip = mRandom.Next(0, 2);
            if (coinFlip == 1)
            {
                flySpeed = -flySpeed;
                flyInitialPosition = new Vector2(mPond.Width + 0.5f * FlyData.Width, flyHeight);
            }
            else
            {
                flyInitialPosition = new Vector2(- 0.5f * FlyData.Width, flyHeight);
            }
            int poolIndex = mFlyPool.Count - 1;
            FlySimState fly = mFlyPool[poolIndex];
            mFlyPool.RemoveAt(poolIndex);
            fly.Set(flySpeed, flyInitialPosition);
            ActiveFlies.Add(fly);
        }

        private float GetRandomInRange(float min, float max)
        {
            return min + (float)mRandom.NextDouble() * (max - min);
        }

        private bool ShouldSpawnFly()
        {
            if (mFlyPool.Count > 0)
            {
                float random = (float)mRandom.NextDouble();
                return (random > mFlyTickSpawnProbability);
            }
            return false;
        }
    }
}
