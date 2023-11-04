using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Sim.Events;

namespace Ranitas.Sim
{
    public sealed class InsectEatingSystem : ISystem
    {
        private struct InsectSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<Rect> Rect;
            public SliceRequirement<Insect> IsInsect;
        }
        private InsectSlice mInsectSlice;

        private struct ToungueSlice
        {
            public SliceRequirementOutput<Rect> Rect;
            public SliceRequirementOutput<ParentEntity> ToungueParent;
            public SliceRequirement<ToungueState> IsToungue;
        }
        private ToungueSlice mToungueSlice;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mInsectSlice);
            registry.SetupSlice(ref mToungueSlice);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int toungueCount = mToungueSlice.Rect.Count;
            for (int toungueIndex = 0; toungueIndex < toungueCount; ++toungueIndex)
            {
                Rect toungueRect = mToungueSlice.Rect[toungueIndex];
                int insectCount = mInsectSlice.Entity.Count;
                for (int insectIndex = insectCount - 1; insectIndex >= 0; --insectIndex)
                {
                    Rect insectRect = mInsectSlice.Rect[insectIndex];
                    if (toungueRect.Intersects(insectRect))
                    {
                        registry.Destroy(mInsectSlice.Entity[insectIndex]);
                        eventSystem.PostMessage(new AteInsect
                        {
                            EatenBy = mToungueSlice.ToungueParent[toungueIndex].Parent,
                            InsectPosition = 0.5f * (insectRect.MaxCorner + insectRect.MinCorner)
                        });
                    }
                }
            }
        }
    }
}
