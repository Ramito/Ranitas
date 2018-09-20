using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Pond;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class WetDryFrogSystem : ISystem
    {
        public WetDryFrogSystem(PondSimState pond)
        {
            mPond = pond;
            const int kExpectedFrogCount = 4;
            mSplashingInFrogs = new List<Entity>(kExpectedFrogCount);
            mSplashingOutFrogs = new List<Entity>(kExpectedFrogCount);
        }

        private struct AirborneFrogs
        {
            public SliceEntityOutput Entities;
            public SliceRequirementOutput<Rect> Rects;
            public SliceRequirement<FrogControlState> IsFrog;   //TODO: Markup components!
            public SliceExclusion<Waterborne> NotWet;
        }
        private AirborneFrogs mAirborneFogs = new AirborneFrogs();

        private struct WaterborneFrogs
        {
            public SliceEntityOutput Entities;
            public SliceRequirement<Waterborne> Waterborne;
            public SliceRequirementOutput<Rect> Rects;
        }
        private WaterborneFrogs mWaterborneFrogs = new WaterborneFrogs();

        //[Dependency]    //TODO: Fancy dpendency injection via reflection?
        private readonly PondSimState mPond;
        private readonly List<Entity> mSplashingInFrogs;
        private readonly List<Entity> mSplashingOutFrogs;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mAirborneFogs);
            registry.SetupSlice(ref mWaterborneFrogs);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            CheckDryFrogs();
            CheckWetFrogs();

            foreach (Entity entity in mSplashingInFrogs)
            {
                registry.RemoveComponent<Gravity>(entity);
                registry.AddComponent(entity, new Waterborne());
            }
            mSplashingInFrogs.Clear();

            foreach (Entity entity in mSplashingOutFrogs)
            {
                registry.RemoveComponent<Waterborne>(entity);
                registry.AddComponent(entity, new Gravity());
            }
            mSplashingOutFrogs.Clear();
        }

        private void CheckDryFrogs()
        {
            int frogCount = mAirborneFogs.Entities.Count;
            for (int i = 0; i < frogCount; ++i)
            {
                float feetHeight = mAirborneFogs.Rects[i].MinY;
                if (feetHeight <= mPond.WaterLevel)
                {
                    mSplashingInFrogs.Add(mAirborneFogs.Entities[i]);
                }
            }
        }

        private void CheckWetFrogs()
        {
            int frogCount = mWaterborneFrogs.Entities.Count;
            for (int i = 0; i < frogCount; ++i)
            {
                float feetHeight = mWaterborneFrogs.Rects[i].MinY;
                if (feetHeight > mPond.WaterLevel)
                {
                    mSplashingOutFrogs.Add(mWaterborneFrogs.Entities[i]);
                }
            }
        }
    }
}
