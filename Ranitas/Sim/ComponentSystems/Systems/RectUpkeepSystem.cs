using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using System;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class RectUpkeepSystem : ISystem
    {
        public RectUpkeepSystem() { }

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            mPostProcessList = new List<Entity>(registry.Capacity);
            registry.SetupSlice(ref mRectNoShape);
            registry.SetupSlice(ref mNoRect);
            registry.SetupSlice(ref mHasRect);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            for (int i = mRectNoShape.Entities.Count - 1; i >= 0; --i)
            {
                registry.RemoveComponent<Rect>(mRectNoShape.Entities[i]);
            }

            int noRectCount = mNoRect.Entities.Count;
            for (int i = 0; i < noRectCount; ++i)
            {
                mPostProcessList.Add(mNoRect.Entities[i]);
            }
            foreach (Entity entity in mPostProcessList)
            {
                registry.AddComponent(entity, new Rect());  //The right data will be updated by the next slice
            }
            mPostProcessList.Clear();

            int hasRectCount = mHasRect.Entities.Count;
            for (int i = 0; i < hasRectCount; ++i)
            {
                Vector2 position = mHasRect.Position[i].Value;
                RectShape rectShape = mHasRect.RectShape[i];
                Rect rect = new Rect(position, rectShape.Width, rectShape.Height);
                registry.SetComponent(mHasRect.Entities[i], rect);
            }
        }

        private struct RectNoShapeSlice
        {
            public SliceEntityOutput Entities;
            public SliceRequirement<Rect> HasRect;
            public SliceExclusion<RectShape> NoShape;
        }
        private RectNoShapeSlice mRectNoShape;

        private struct ShapePositionNoRectSlice
        {
            public SliceEntityOutput Entities;
            public SliceRequirement<RectShape> HasRectShape;
            public SliceRequirement<Position> HasPosition;
            public SliceExclusion<Rect> NoRect;
        }
        private ShapePositionNoRectSlice mNoRect;
        private List<Entity> mPostProcessList;

        private struct ShapePositionRectSlice
        {
            public SliceEntityOutput Entities;
            public SliceRequirementOutput<RectShape> RectShape;
            public SliceRequirementOutput<Position> Position;
            public SliceRequirement<Rect> HasRect;
        }
        private ShapePositionRectSlice mHasRect;
    }
}
