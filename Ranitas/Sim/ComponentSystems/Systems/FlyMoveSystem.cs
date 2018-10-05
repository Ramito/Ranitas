using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Data;
using Ranitas.Pond;
using System;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class FlyMoveSystem : ISystem
    {
        public FlyMoveSystem(FrameTime time, FlyData flyData)
        {
            mTime = time;
            mPositionBuffer = new Position[flyData.MaxActiveFlies];
        }

        private FrameTime mTime;
        private Position[] mPositionBuffer;

        private struct MovingFliesSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<Position> Position;
            public SliceRequirementOutput<Velocity> Velocity;
            public SliceRequirement<Insect> IsInsect;
        }
        private MovingFliesSlice mMovingFliesSlice;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mMovingFliesSlice);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int count = mMovingFliesSlice.Entity.Count;
            for (int i = 0; i < count; ++i)
            {
                Vector2 newPosition = mMovingFliesSlice.Position[i].Value + mTime.DeltaTime * mMovingFliesSlice.Velocity[i].Value;
                mPositionBuffer[i] = new Position(newPosition);
            }
            //TODO: Is it *really* more efficient to use a buffer?
            for (int i = 0; i < count; ++i)
            {
                registry.SetComponent(mMovingFliesSlice.Entity[i], mPositionBuffer[i]);
            }
        }
    }
}
