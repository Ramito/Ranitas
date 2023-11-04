using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Data;
using Ranitas.Sim;
using System.Collections.Generic;

namespace Ranitas.Render
{
    public sealed class FlyAnimationSystem : ISystem
    {
        private FrameTime mTime;
        private FlyData mFlyData;

        private List<Entity> mWings = new List<Entity>();
        private List<WingPayload> mPayloads = new List<WingPayload>();

        public FlyAnimationSystem(RanitasDependencies dependencies)
        {
            mTime = dependencies.Time;
            mFlyData = dependencies.FlyData;
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
            foreach (Entity wing in mWings)
            {
                registry.Destroy(wing);
            }
            mWings.Clear();

            for (int i = 0; i < mFreshFlySlice.Entity.Count; ++i)
            {
                registry.AddComponent(mFreshFlySlice.Entity[i], new WingTimeStamp { Time = mTime.CurrentGameTime });
            }

            for (int i = 0; i < mFlySlice.Entity.Count; ++i)
            {
                float timeDelta = mTime.CurrentGameTime - mFlySlice.TimeStamp[i].Time;
                if (timeDelta % 0.075f < 0.025f)
                {
                    continue;
                }
                float offsetX = mFlyData.WingSize;
                if (mFlySlice.Velocity[i].Value.X < 0f)
                {
                    offsetX *= -1f;
                }
                Vector2 offset = new Vector2(-offsetX, mFlyData.WingSize);
                Rect wingRect = mFlySlice.Rect[i].Inflated(mFlyData.WingSize).Translated(offset);
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
