using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Core.EventSystem;
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
            public SliceRequirement<Airborne> Airborne;
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

        private struct FallingFrogs
        {
            public SliceEntityOutput Entities;
            public SliceRequirement<Airborne> Airborne;
            public SliceRequirementOutput<Position> Positions;
            public SliceRequirementOutput<RectShape> Shapes;
        }

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
            UpdateWetDryState(registry);
            CheckLillyCollisions(registry);
            //UPDATE AIR PHYSICS AND WATER PHYSICS!
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
                if (mAirborneFogs.Velocities[i].Value.Y < 0f)
                {
                    Rect frogRect = FrogRect(mAirborneFogs.Positions[i], mAirborneFogs.Shapes[i]);
                    foreach (LilyPadSimState lilypad in mPond.Lilies)
                    {
                        if (frogRect.Intersects(lilypad.Rect))
                        {
                            mLandingFrogs.Add(mAirborneFogs.Entities[i]);
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
                //TODO: Check lilly pad collision!

                Vector2 feetPosition = FrogFeetPosition(mAirborneFogs.Positions[i], mAirborneFogs.Shapes[i]);
                if (feetPosition.Y < mPond.WaterLevel)
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
                //TODO: Check lilly pad collision!

                Vector2 feetPosition = FrogFeetPosition(mWaterborneFrogs.Positions[i], mWaterborneFrogs.Shapes[i]);
                if (feetPosition.Y < mPond.WaterLevel)
                {
                    mSplashingOutFrogs.Add(mWaterborneFrogs.Entities[i]);
                }
            }
        }

        private static Rect FrogRect(Position position, RectShape shape)
        {
            return new Rect(position.Value, shape.Width, shape.Height);
        }

        private static Vector2 FrogFeetPosition(Position position, RectShape shape)
        {
            return position.Value - new Vector2(0f, shape.Height * 0.5f);
        }
    }
}
