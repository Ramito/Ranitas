using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Pond;
using System;

namespace Ranitas.Sim
{
    class WaterSystem : ISystem
    {
        public WaterSystem(FrameTime time, PondSimState pond)
        {
            mPond = pond;
            mTime = time;
            for (int i = 0; i < mWaterPositions.Length; ++i)
            {
                mWaterPositions[i] = mPond.WaterLevel;
                mWaterVelocities[i] = 0f;
            }
            mPond.WaterPositions = mWaterPositions;
            mPond.WaterVelocities = mWaterVelocities;
            mWaterDX = mPond.Width / kWaterResolution;
        }

        const int kWaterResolution = 2 * 1024;
        const float kWaterSpring = 150.5f;
        const float kWaterDiffusion = 7000f;
        const float kWaterDamp = 0.125f;
        const float kWaterViscosity = 8.5f;
        float mWaterDX;
        private float[] mWaterPositions = new float[kWaterResolution];
        private float[] mWaterVelocities = new float[kWaterResolution];
        private PondSimState mPond;
        private FrameTime mTime;

        private struct SplashingEntities
        {
            public SliceRequirementOutput<Velocity> Velocities;
            public SliceRequirementOutput<Rect> Rects;
        }
        private SplashingEntities mSplashing = new SplashingEntities();

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mSplashing);
        }

        private void IntegrateWater()
        {
            float dampingFactor = (float)Math.Exp(-mTime.DeltaTime * kWaterDamp);
            float previousPosition = mPond.WaterLevel;
            for (int i = 0; i < mWaterPositions.Length - 1; ++i)
            {
                float currentPosition = mWaterPositions[i];
                float nextPosition = mWaterPositions[i + 1];
                float diffusion = (nextPosition - 2f * currentPosition + previousPosition) * kWaterDiffusion / (mWaterDX * mWaterDX);
                float spring = (mPond.WaterLevel - currentPosition) * kWaterSpring;
                mWaterVelocities[i] = (mWaterVelocities[i] + (spring + diffusion) * mTime.DeltaTime) * dampingFactor;
                mWaterPositions[i] += mTime.DeltaTime * mWaterVelocities[i];
                previousPosition = currentPosition;
            }
            {
                int lastIndex = mWaterPositions.Length - 1;
                float currentPosition = mWaterPositions[lastIndex];
                float nextPosition = mPond.WaterLevel;
                float diffusion = (nextPosition - 2f * currentPosition + previousPosition) * kWaterDiffusion / (mWaterDX * mWaterDX);
                float spring = (mPond.WaterLevel - currentPosition) * kWaterSpring;
                mWaterVelocities[lastIndex] = (mWaterVelocities[lastIndex] + (spring + diffusion) * mTime.DeltaTime) * dampingFactor;
                mWaterPositions[lastIndex] += mTime.DeltaTime * mWaterVelocities[lastIndex];
            }
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            for (int iSplash = 0; iSplash < mSplashing.Rects.Count; ++iSplash)
            {
                Rect splashingRect = mSplashing.Rects[iSplash];
                int minWater = Math.Max((int)(splashingRect.MinX / mWaterDX), 0);
                int maxWater = Math.Min((int)(splashingRect.MaxX / mWaterDX), mWaterPositions.Length);
                for (int iWater = minWater; iWater < maxWater; ++iWater)
                {
                    Vector2 waterPos = new Vector2(iWater * mWaterDX, mWaterPositions[iWater]);
                    if (splashingRect.Contains(waterPos))
                    {
                        float verticalSplashVelocity = mSplashing.Velocities[iSplash].Value.Y;
                        float velocityDifference = verticalSplashVelocity - mWaterVelocities[iSplash];
                        float velocityChange = mTime.DeltaTime * velocityDifference * kWaterViscosity;
                        mWaterVelocities[iWater] += velocityChange;
                    }
                }
            }

            IntegrateWater();
        }
    }
}
