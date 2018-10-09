using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Data;
using System;

namespace Ranitas.Sim
{
    public sealed class FlyNoiseSystem : ISystem
    {
        private const float kFrequencyNormalizer = (float)(2d *Math.PI);
        private const float kGoldenRatio = 1.61803398874989484820458683436f;    //Esoteric factor used to decouple horizontal and vertical frequencies

        public FlyNoiseSystem(FrameTime time, FlyData flyData, FlyNoiseData noiseData)
        {
            mFlyNoiseData = noiseData;
            mRandom = new Random();
            mTime = time;
            mNoiseStateBuffer = new FlyNoiseState[flyData.MaxActiveFlies];
            mPositionBuffer = new Position[flyData.MaxActiveFlies];
        }

        private FlyNoiseData mFlyNoiseData;
        private Random mRandom; //TODO: These randoms could be a problem one day, maybe it is time to share them
        private FrameTime mTime;
        private FlyNoiseState[] mNoiseStateBuffer;
        private Position[] mPositionBuffer;

        private struct NoNoiseFlies
        {
            public SliceEntityOutput Entity;
            public SliceExclusion<FlyNoiseState> NoNoise;
            public SliceRequirement<Insect> IsInsect;
        }
        private NoNoiseFlies mNoNoiseSlice;

        private struct NoisyFlies
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<FlyNoiseState> NoiseState;
            public SliceRequirementOutput<Position> Position;
            public SliceRequirement<Insect> IsInsect;
        }
        private NoisyFlies mNoisyFliesSlice;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mNoNoiseSlice);
            registry.SetupSlice(ref mNoisyFliesSlice);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int noNoiseCount = mNoNoiseSlice.Entity.Count;
            for (int i = noNoiseCount - 1; i >= 0; --i)
            {
                float xModule = mRandom.GetRandomInRange(0f, kFrequencyNormalizer);
                float yModule = mRandom.GetRandomInRange(0f, kFrequencyNormalizer);
                float xPhase = mRandom.GetRandomInRange(0f, kFrequencyNormalizer);
                float yPhase = mRandom.GetRandomInRange(0f, kFrequencyNormalizer);
                FlyNoiseState initialState = new FlyNoiseState(xModule, yModule, xPhase, yPhase);
                registry.AddComponent(mNoNoiseSlice.Entity[i], initialState);
            }

            int noiseCount = mNoisyFliesSlice.Entity.Count;
            for (int i = 0; i < noiseCount; ++i)
            {
                float deltaTime = mTime.DeltaTime;
                FlyNoiseState state = mNoisyFliesSlice.NoiseState[i];

                float frequencyAdjust = kFrequencyNormalizer * mFlyNoiseData.ModulatorFrquency;
                state.XModulePhase += (frequencyAdjust * deltaTime);
                state.YModulePhase += (frequencyAdjust * kGoldenRatio * deltaTime);

                frequencyAdjust = kFrequencyNormalizer * mFlyNoiseData.OscilationFrequency;
                state.XOscilationPhase += (frequencyAdjust * deltaTime);
                state.YOscilationPhase += (frequencyAdjust * kGoldenRatio * deltaTime);

                mNoiseStateBuffer[i] = state;
            }

            //Compute buffer effects
            for (int i = 0; i < noiseCount; ++i)
            {
                FlyNoiseState previousState = mNoisyFliesSlice.NoiseState[i];
                FlyNoiseState currentState = mNoiseStateBuffer[i];

                float xCurrent = (float)(mFlyNoiseData.OscilationModule * Math.Cos(currentState.XModulePhase) * Math.Cos(currentState.XOscilationPhase));
                float yCurrent = (float)(mFlyNoiseData.OscilationModule * Math.Cos(currentState.YModulePhase) * Math.Cos(currentState.YOscilationPhase));

                float xPrevious = (float)(mFlyNoiseData.OscilationModule * Math.Cos(previousState.XModulePhase) * Math.Cos(previousState.XOscilationPhase));
                float yPrevious = (float)(mFlyNoiseData.OscilationModule * Math.Cos(previousState.YModulePhase) * Math.Cos(previousState.YOscilationPhase));

                float xDelta = xCurrent - xPrevious;
                float yDelta = yCurrent - yPrevious;

                Vector2 currentPosition = mNoisyFliesSlice.Position[i].Value;
                Vector2 newPosition = currentPosition + new Vector2(xDelta, yDelta);

                mPositionBuffer[i] = new Position(newPosition);
            }

            //Write buffers back to components
            for (int i = 0; i < noiseCount; ++i)
            {
                Entity fly = mNoisyFliesSlice.Entity[i];
                registry.SetComponent(fly, mNoiseStateBuffer[i]);
                registry.SetComponent(fly, mPositionBuffer[i]);
            }
        }
    }
}
