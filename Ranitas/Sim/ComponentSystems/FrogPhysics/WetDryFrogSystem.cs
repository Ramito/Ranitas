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
            mLandingFrogs = new List<Entity>(kExpectedFrogCount);
        }

        private struct AirborneFrogs
        {
            public SliceEntityOutput Entities;
            public SliceExclusion<Waterborne> NotWet;
            public SliceRequirementOutput<Position> Positions;
            public SliceRequirementOutput<Velocity> Velocities;
            public SliceRequirementOutput<RectShape> Shapes;
        }
        private AirborneFrogs mAirborneFogs = new AirborneFrogs();

        private struct WaterborneFrogs
        {
            public SliceEntityOutput Entities;
            public SliceRequirement<Waterborne> Waterborne;
            public SliceRequirementOutput<Position> Positions;
            public SliceRequirementOutput<RectShape> Shapes;
        }
        private WaterborneFrogs mWaterborneFrogs = new WaterborneFrogs();

        //[Dependency]    //TODO: Fancy dpendency injection via reflection?
        private readonly PondSimState mPond;
        private readonly List<Entity> mSplashingInFrogs;
        private readonly List<Entity> mSplashingOutFrogs;
        private readonly List<Entity> mLandingFrogs;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mAirborneFogs);
            registry.SetupSlice(ref mWaterborneFrogs);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            CheckLillyCollisions(registry);
            UpdateWetDryState(registry);
        }

        private void UpdateWetDryState(EntityRegistry registry)
        {
            CheckDryFrogs();
            CheckWetFrogs();

            foreach (Entity entity in mSplashingInFrogs)
            {
                registry.RemoveComponent<Airborne>(entity);
                registry.AddComponent(entity, new Waterborne());
            }
            mSplashingInFrogs.Clear();

            foreach (Entity entity in mSplashingOutFrogs)
            {
                registry.RemoveComponent<Waterborne>(entity);
                registry.AddComponent(entity, new Airborne());
            }
            mSplashingOutFrogs.Clear();
        }

        private void CheckLillyCollisions(EntityRegistry registry)
        {
            int airborneFrogCount = mAirborneFogs.Entities.Count;
            for (int i = 0; i < airborneFrogCount; ++i)
            {
                if (mAirborneFogs.Velocities[i].Value.Y <= 0f)
                {
                    Rect frogRect = CommonFrogProperties.FrogRect(mAirborneFogs.Positions[i], mAirborneFogs.Shapes[i]);
                    foreach (LilyPadSimState lilypad in mPond.Lilies)
                    {
                        if (frogRect.Intersects(lilypad.Rect))
                        {
                            Entity landingFrog = mAirborneFogs.Entities[i];
                            if (registry.HasComponent<Airborne>(landingFrog))
                            {
                                mLandingFrogs.Add(landingFrog);
                            }
                            registry.SetComponent(landingFrog, new Velocity());
                            Vector2 landedPosition = mAirborneFogs.Positions[i].Value;
                            landedPosition.Y = lilypad.Rect.MaxY + (mAirborneFogs.Shapes[i].Height * 0.5f);
                            registry.SetComponent(landingFrog, new Position(landedPosition));
                            break;
                        }
                    }
                }
            }
            foreach (Entity entity in mLandingFrogs)
            {
                registry.RemoveComponent<Airborne>(entity);
                registry.AddComponent(entity, new Landed());
            }
            mLandingFrogs.Clear();
        }

        private void CheckDryFrogs()
        {
            int frogCount = mAirborneFogs.Entities.Count;
            for (int i = 0; i < frogCount; ++i)
            {
                Vector2 feetPosition = CommonFrogProperties.FrogFeetPosition(mAirborneFogs.Positions[i], mAirborneFogs.Shapes[i]);
                if (feetPosition.Y <= mPond.WaterLevel)
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
                Vector2 feetPosition = CommonFrogProperties.FrogFeetPosition(mWaterborneFrogs.Positions[i], mWaterborneFrogs.Shapes[i]);
                if (feetPosition.Y > mPond.WaterLevel)
                {
                    mSplashingOutFrogs.Add(mWaterborneFrogs.Entities[i]);
                }
            }
        }
    }
}
