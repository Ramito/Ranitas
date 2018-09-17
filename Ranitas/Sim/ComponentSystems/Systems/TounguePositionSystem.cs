using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using System.Diagnostics;

namespace Ranitas.Sim
{
    public sealed class TounguePositionSystem : ISystem
    {
        public TounguePositionSystem(Data.FrogData data)
        {
            mShapeData = new ToungueShapeData(data);
        }

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mTounguesWithPosition);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int count = mTounguesWithPosition.Entity.Count;
            for (int i = 0; i < count; ++i)
            {
                Entity parentFrog = mTounguesWithPosition.ParentFrog[i].Parent;
                Debug.Assert(registry.IsValid(parentFrog));
                Debug.Assert(registry.HasComponent<Position>(parentFrog));
                Debug.Assert(registry.HasComponent<RectShape>(parentFrog));
                Debug.Assert(registry.HasComponent<Facing>(parentFrog));
                Rect frogRect = CommonFrogProperties.FrogRect(registry.GetComponent<Position>(parentFrog), registry.GetComponent<RectShape>(parentFrog));
                int facing = registry.GetComponent<Facing>(parentFrog).CurrentFacing;
                Vector2 extendDirection;
                Vector2 anchor;
                if (facing >= 0f)
                {
                    extendDirection = Vector2.UnitX;
                    anchor = frogRect.MaxCorner;
                }
                else
                {
                    extendDirection = -Vector2.UnitX;
                    anchor = frogRect.MinMaxCorner;
                }
                RectShape toungueShape = mTounguesWithPosition.CurrentShape[i];
                anchor -= (toungueShape.Height * mShapeData.RelativeVerticalOffset * Vector2.UnitY);
                anchor += toungueShape.Width * extendDirection * 0.5f;
                Entity toungueEntity = mTounguesWithPosition.Entity[i];
                registry.SetComponent<Position>(toungueEntity, new Position(anchor));
            }
        }

        private struct TounguesWithPosition
        {
            public SliceEntityOutput Entity;    //WIP TODO: Interface to find slice index for given index, and method to determine if the entity is in the slice!
            public SliceRequirementOutput<RectShape> CurrentShape;
            public SliceRequirementOutput<ParentEntity> ParentFrog;
            public SliceRequirement<ToungueState> ToungueState;
            public SliceRequirement<Position> Position;
        }
        private TounguesWithPosition mTounguesWithPosition;

        private ToungueShapeData mShapeData;
    }
}
