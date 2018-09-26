using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Data;

namespace Ranitas.Sim
{
    public sealed class FrogAnimationSystem : ISystem
    {
        public FrogAnimationSystem(FrogAnimationData data)
        {
            mData = data;
            mFrameWidth = 0.25f;    // 1 over number of frames
        }

        private struct LandedFrogSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirement<AnimationState> HasAnimation;
            public SliceRequirement<Landed> IsLanded;
        }
        private LandedFrogSlice mLandedFrog;

        private struct JumpingFrogSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirement<AnimationState> HasAnimation;
            public SliceExclusion<Landed> NotLanded;
            public SliceExclusion<Waterborne> NotSwiming;
        }
        private JumpingFrogSlice mJumpingFrogSlice;

        private struct SwimingFrogSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<Waterborne> Waterborne;
            public SliceRequirement<AnimationState> HasAnimation;
        }
        private SwimingFrogSlice mSwimingFrogSlice;

        private struct FacingSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<Facing> Facing;
            public SliceRequirementOutput<AnimationState> Animation;
        }
        private FacingSlice mFacingSlice;

        private FrogAnimationData mData;
        private float mFrameWidth;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mLandedFrog);
            registry.SetupSlice(ref mJumpingFrogSlice);
            registry.SetupSlice(ref mSwimingFrogSlice);
            registry.SetupSlice(ref mFacingSlice);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            SetAnimationFrames(registry, mLandedFrog.Entity, mData.LandedFrame);
            SetAnimationFrames(registry, mJumpingFrogSlice.Entity, mData.JumpingFrame);
            UpdateSwimAnimation(registry);
            UpdateFacing(registry);
        }

        private void SetAnimationFrames(EntityRegistry registry, SliceEntityOutput entities, int frameIndex)
        {
            int count = entities.Count;
            for (int i = 0; i < count; ++i)
            {
                SetAnimationFrame(registry, entities[i], frameIndex);
            }
        }

        private void SetAnimationFrame(EntityRegistry registry, Entity entity, int frameIndex)
        {
            float minX = frameIndex * mFrameWidth;
            float maxX = minX + mFrameWidth;
            registry.SetComponent(entity, new AnimationState(minX, maxX));
        }

        private void UpdateSwimAnimation(EntityRegistry registry)
        {
            int swimCount = mSwimingFrogSlice.Entity.Count;
            for (int i = 0; i < swimCount; ++i)
            {
                Waterborne waterBorne = mSwimingFrogSlice.Waterborne[i];
                int frame;
                if (waterBorne.SwimKickPhase <= 0)
                {
                    frame = mData.SwimingFrame;
                }
                else
                {
                    frame = mData.FloatingFrame;
                }
                SetAnimationFrame(registry, mSwimingFrogSlice.Entity[i], frame);
            }
        }

        private void UpdateFacing(EntityRegistry registry)
        {
            int count = mFacingSlice.Entity.Count;
            for (int i = 0; i < count; ++i)
            {
                if (mFacingSlice.Facing[i].CurrentFacing < 0)
                {
                    AnimationState animState = mFacingSlice.Animation[i];
                    float swap = animState.MinX;
                    animState.MinX = animState.MaxX;
                    animState.MaxX = swap;
                    registry.SetComponent(mFacingSlice.Entity[i], animState);
                }
            }
        }
    }
}
