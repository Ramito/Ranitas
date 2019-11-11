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
            for (int i = 0; i < mWaterHeight.Length; ++i)
            {
                mWaterHeight[i] = mPond.WaterLevel;
                mWaterFlow[i] = 0f;
            }
            mPond.WaterPositions = mWaterHeight;
            mPond.WaterVelocities = mWaterFlow;
            mWaterDX = mPond.Width / kWaterResolution;
        }

        const int kWaterResolution = 225;
        const float kWaterViscosity = 1.5f;
        float mWaterDX;
        private float[] mWaterHeight = new float[kWaterResolution];
        private float[] mWaterFlow = new float[kWaterResolution  + 1];
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

        private void IntegrateHeight(float deltaTime)
        {
            float previousHeight = mPond.WaterLevel;
            float currentHeight = mWaterHeight[0];
            for (int i = 0; i < mWaterHeight.Length; ++i)
            {
                bool lastIndex = (i == mWaterHeight.Length - 1);

                float nextHeight = (lastIndex) ? mPond.WaterLevel : mWaterHeight[i + 1];
                float leftFlow = mWaterFlow[i];
                float leftHeight = (leftFlow > 0f) ? previousHeight : currentHeight;
                float rightFlow = mWaterFlow[i + 1];
                float rightHeight = (rightFlow > 0f) ? currentHeight : nextHeight;

                float heightSpeed = ((leftHeight * leftFlow) - (rightHeight * rightFlow)) / mWaterDX;
                mWaterHeight[i] = currentHeight + (heightSpeed * deltaTime) * 0.17477575f;

                previousHeight = currentHeight;
                currentHeight = nextHeight;
            }
        }

        private void IntegrateFlow(float deltaTime)
        {
            float previousHeight = mPond.WaterLevel;
            for (int i = 0; i < mWaterFlow.Length; ++i)
            {
                float currentHeight = (i < mWaterHeight.Length) ? mWaterHeight[i] : mPond.WaterLevel;
                mWaterFlow[i] = (mWaterFlow[i] - 760f * (currentHeight - previousHeight) * (deltaTime / mWaterDX)) * 0.99835225f;
                previousHeight = currentHeight;
            }
        }

        private void IntegrateWater()
        {
            const int iterations = 10;
            float deltaTime = mTime.DeltaTime / iterations;
            for (int i = 0; i < iterations; ++i)
            {
                IntegrateFlow(deltaTime);
                IntegrateHeight(deltaTime);
            }
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            for (int iSplash = 0; iSplash < mSplashing.Rects.Count; ++iSplash)
            {
                float horizontalSplashVelocity = mSplashing.Velocities[iSplash].Value.X;
                float verticalSplashVelocity = mSplashing.Velocities[iSplash].Value.Y;
                Rect splashingRect = mSplashing.Rects[iSplash];
                int minWater = Math.Max((int)(splashingRect.MinX / mWaterDX), 0);
                int maxWater = Math.Min((int)(splashingRect.MaxX / mWaterDX), mWaterHeight.Length);
                for (int iWater = minWater; iWater < maxWater; ++iWater)
                {
                    Vector2 waterPos = new Vector2(iWater * mWaterDX, mWaterHeight[iWater]);
                    if (splashingRect.Contains(waterPos))
                    {
                        float velocityChange = mTime.DeltaTime * verticalSplashVelocity * kWaterViscosity;
                        mWaterHeight[iWater] += velocityChange;
                        float flowChange = mTime.DeltaTime * (horizontalSplashVelocity - mWaterFlow[iWater]) * 15f * kWaterViscosity;
                        mWaterFlow[iWater] += flowChange;
                    }
                }
            }

            IntegrateWater();
        }
    }
}
