using Microsoft.Xna.Framework;
using Ranitas.Core.ECS;
using Ranitas.Core.EventSystem;
using Ranitas.Pond;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class WetDryFrogSystem
    {
        private struct AirborneFrogs
        {
            public ValueRegistry<Airborne> Airborne;
            public ValueRegistry<Position> Positions;
            public ValueRegistry<FrogShape> Shapes;
        }
        private AirborneFrogs mAirborneFogs;

        private struct WaterborneFrogs
        {
            public ValueRegistry<Waterborne> Waterborne;
            public ValueRegistry<Position> Positions;
            public ValueRegistry<FrogShape> Shapes;
        }
        private WaterborneFrogs mWaterborneFrogs;

        private PondSimState mPond;
        private List<Entity> mSplashingInFrogs = new List<Entity>(4);
        private List<Entity> mSplashingOutFrogs = new List<Entity>(4);

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.ConfigureSlice()
                .Require(mAirborneFogs.Airborne)
                .Require(mAirborneFogs.Positions)
                .Require(mAirborneFogs.Shapes)
                .CreateSlice();

            registry.ConfigureSlice()
                .Require(mWaterborneFrogs.Waterborne)
                .Require(mWaterborneFrogs.Positions)
                .Require(mWaterborneFrogs.Shapes)
                .CreateSlice();
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            uint frogCount = mAirborneFogs.Airborne.Count;
            for (uint i = 0; i < frogCount; ++i)
            {
                //TODO: Check lilly pad collision!

                Vector2 feetPosition = FrogFeetPosition(mAirborneFogs.Positions[i], mAirborneFogs.Shapes[i]);
                if (feetPosition.Y < mPond.WaterLevel)
                {
                    //SET FROG TO WET!
                    //mSplashingInFrogs.Add();    //NEED TO BE ABLE TO GET THE ENTITY!
                }
                else
                {
                    //Set frog to dry!
                }
            }
        }

        private static Vector2 FrogFeetPosition(Position position, FrogShape shape)
        {
            return position.Value - new Vector2(0f, shape.Height * 0.5f);
        }
    }
}
