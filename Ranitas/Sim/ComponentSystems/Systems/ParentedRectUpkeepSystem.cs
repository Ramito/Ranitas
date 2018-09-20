using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;

namespace Ranitas.Sim
{
    public sealed class ParentedRectUpkeepSystem : ISystem
    {
        public ParentedRectUpkeepSystem() { }

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mHasParentedRect);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int hasRectCount = mHasParentedRect.Entities.Count;
            for (int i = 0; i < hasRectCount; ++i)
            {
                Vector2 position = mHasParentedRect.Position[i].Value;
                RectShape rectShape = mHasParentedRect.RectShape[i];
                Rect rect = new Rect(position, rectShape.Width, rectShape.Height);
                registry.SetComponent(mHasParentedRect.Entities[i], rect);
            }
        }

        private struct ShapePositionRectSlice
        {
            public SliceEntityOutput Entities;
            public SliceRequirementOutput<RectShape> RectShape;
            public SliceRequirementOutput<Position> Position;
            public SliceRequirement<Rect> HasRect;
            public SliceRequirement<ParentEntity> NoParents;
        }
        private ShapePositionRectSlice mHasParentedRect;
    }
}
