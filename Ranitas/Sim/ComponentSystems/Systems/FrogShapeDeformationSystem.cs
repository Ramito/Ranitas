using Ranitas.Core;
using Ranitas.Core.ECS;

namespace Ranitas.Sim
{
    public sealed class FrogShapeDeformationSystem : ISystem
    {
        public FrogShapeDeformationSystem(Data.FrogData frogData)
        {
            mOriginalFrogShape = new RectShape(frogData.Width, frogData.Height);
            mDeformationData = new FrogShapeDeformationData(frogData);
        }

        private RectShape mOriginalFrogShape;
        private FrogShapeDeformationData mDeformationData;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mLandedFrogs);
            registry.SetupSlice(ref mAirborneFrogs);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int count = mLandedFrogs.Frog.Count;
            for (int i = 0; i < count; ++i)
            {
                float relativeSquish = mLandedFrogs.Landed[i].RelativeJumpPower;
                if (relativeSquish != 0)
                {
                    float scale = relativeSquish * mDeformationData.JumpSquish + (1f - relativeSquish);
                    float height = scale * mOriginalFrogShape.Height;
                    float width = mOriginalFrogShape.Width / scale;
                    RectShape newShape = new RectShape(width, height);
                    registry.SetComponent(mLandedFrogs.Frog[i], newShape);
                }
            }
            count = mAirborneFrogs.Frog.Count;
            for (int i = 0; i < count; ++i)
            {
                if (mAirborneFrogs.Shape[i].Width != mOriginalFrogShape.Width)  //Assumption is we preserve volume, so comparing just this is enough
                {
                    registry.SetComponent(mAirborneFrogs.Frog[i], mOriginalFrogShape);
                }
            }
        }

        private struct LandedFrogsSlice
        {
            public SliceEntityOutput Frog;
            public SliceRequirement<RectShape> HaveShape;
            public SliceRequirementOutput<Landed> Landed;
        }
        private LandedFrogsSlice mLandedFrogs;

        private struct AirborneFrogs
        {
            public SliceEntityOutput Frog;
            public SliceRequirementOutput<RectShape> Shape;
            public SliceRequirement<Airborne> AreAirborne;
        }
        private AirborneFrogs mAirborneFrogs;
    }
}
