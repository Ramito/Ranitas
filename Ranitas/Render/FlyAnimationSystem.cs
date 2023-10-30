using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Sim;
using System.Collections.Generic;

namespace Ranitas.Render
{
    public sealed class FlyAnimationSystem : ISystem
    {
        private FrameTime mTime;

        private float mTotalTime = 0f;
        private List<Entity> mWings = new List<Entity>();
        private List<WingPayload> mPayloads = new List<WingPayload>();

        public FlyAnimationSystem(FrameTime time)
        {
            mTime = time;
        }

        private struct FreshFlySlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirement<Insect> IsFly;
            public SliceExclusion<WingTimeStamp> NoTimeStamp;
        }
        private FreshFlySlice mFreshFlySlice;

        private struct FlySlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirement<Insect> IsFly;
            public SliceRequirementOutput<Rect> Rect;
            public SliceRequirementOutput<Position> Position;
            public SliceRequirementOutput<WingTimeStamp> TimeStamp;
            public SliceRequirementOutput<Velocity> Velocity;
        }
        private FlySlice mFlySlice;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mFreshFlySlice);
            registry.SetupSlice(ref mFlySlice);
        }

        private struct WingPayload
        {
            public Rect Rect;
            public Position Position;
        }

        private struct WingTimeStamp
        {
            public float Time;
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            mTotalTime += mTime.DeltaTime;

            foreach (Entity wing in mWings)
            {
                registry.Destroy(wing);
            }
            mWings.Clear();

            for (int i = 0; i < mFreshFlySlice.Entity.Count; ++i)
            {
                registry.AddComponent(mFreshFlySlice.Entity[i], new WingTimeStamp { Time = mTotalTime });
            }

            for (int i = 0; i < mFlySlice.Entity.Count; ++i)
            {
                float timeDelta = mTotalTime - mFlySlice.TimeStamp[i].Time;
                if (timeDelta % 0.075f < 0.025f)
                {
                    continue;
                }
                float wingSize = 1.75f;
                float offsetX = wingSize;
                if (mFlySlice.Velocity[i].Value.X < 0f)
                {
                    offsetX *= -1f;
                }
                Vector2 offset = new Vector2(-offsetX, wingSize);
                Rect wingRect = mFlySlice.Rect[i].Inflated(wingSize).Translated(offset);
                WingPayload payload = new WingPayload
                {
                    Rect = wingRect,
                    Position = mFlySlice.Position[i],
                };
                mPayloads.Add(payload);
            }

            foreach (WingPayload payload in mPayloads)
            {
                Entity wing = registry.Create();
                registry.AddComponent(wing, payload.Rect);
                registry.AddComponent(wing, payload.Position);
                registry.AddComponent(wing, Color.LightGray);
                mWings.Add(wing);
            }

            mPayloads.Clear();
        }
    }
}
