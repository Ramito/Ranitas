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
        private const float kModulatorFrquency = 0.65f;
        private const float kOscilationFrequency = 9f;//6.5f;
        private const float kOscilationModule = 6f;
        const double GoldenRatio = 1.61803398874989484820458683436;

        public FlyNoiseSystem(FrameTime time, FlyData flyData)
        {
            mRandom = new Random();
            mTime = time;
            mNoiseStateBuffer = new FlyNoiseState[flyData.MaxActiveFlies];
            mPositionBuffer = new Position[flyData.MaxActiveFlies];
        }

        private Random mRandom; //TODO: These randoms could be a problem one day, maybe it is time to share them
        private FrameTime mTime;
        private FlyNoiseState[] mNoiseStateBuffer;
        private Position[] mPositionBuffer;

        public struct FlyNoiseState
        {
            public FlyNoiseState(float xModule, float yModule, float xOscilation, float yOscilation)
            {
                XModulePhase = xModule;
                YModulePhase = yModule;
                XOscilationPhase = xOscilation;
                YOscilationPhase = yOscilation;
            }

            public float XModulePhase;
            public float YModulePhase;
            public float XOscilationPhase;
            public float YOscilationPhase;
        }

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

                float frequencyAdjust = kFrequencyNormalizer * kModulatorFrquency;
                state.XModulePhase += (frequencyAdjust * deltaTime);
                state.YModulePhase += (frequencyAdjust * deltaTime);

                frequencyAdjust = kFrequencyNormalizer * kOscilationFrequency;
                state.XOscilationPhase += (frequencyAdjust * deltaTime);
                state.YOscilationPhase += (frequencyAdjust * deltaTime);

                mNoiseStateBuffer[i] = state;
            }

            //Compute buffer effects
            for (int i = 0; i < noiseCount; ++i)
            {
                FlyNoiseState previousState = mNoisyFliesSlice.NoiseState[i];
                FlyNoiseState currentState = mNoiseStateBuffer[i];

                float xCurrent = (float)(kOscilationModule * Math.Cos(currentState.XModulePhase) * Math.Cos(currentState.XModulePhase));
                float yCurrent = (float)(kOscilationModule * Math.Cos(currentState.YModulePhase * GoldenRatio) * Math.Cos(currentState.YModulePhase * GoldenRatio));

                float xPrevious = (float)(kOscilationModule * Math.Cos(previousState.XModulePhase) * Math.Cos(previousState.XModulePhase));
                float yPrevious = (float)(kOscilationModule * Math.Cos(previousState.YModulePhase * GoldenRatio) * Math.Cos(previousState.YModulePhase * GoldenRatio));

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
