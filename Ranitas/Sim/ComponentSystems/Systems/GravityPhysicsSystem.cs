using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Core.Physics;
using Ranitas.Pond;

namespace Ranitas.Sim
{
    public sealed class GravityPhysicsSystem : ISystem
    {
        public GravityPhysicsSystem(FrameTime frameTime)
        {
            mTime = frameTime;
        }

        private FrameTime mTime;

        private struct FallingEntities
        {
            public SliceEntityOutput Entities;
            public SliceRequirementOutput<Position> Positions;
            public SliceRequirementOutput<Velocity> Velocities;
            public SliceRequirement<Gravity> HasGravity;   //TODO: Markup components!
        }
        private FallingEntities mFallingEntities = new FallingEntities();

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mFallingEntities);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int count = mFallingEntities.Entities.Count;
            for (int i = 0; i < count; ++i)
            {
                Vector2 velocityDelta = Dynamics.NewtonianVelocityDelta(PondSimState.kGravity, mTime);  //TODO: Data drive gravity!
                Vector2 newVelocity = mFallingEntities.Velocities[i].Value + velocityDelta;
                Vector2 frogPositionDelta = Dynamics.NewtonianPositionDelta(mFallingEntities.Velocities[i].Value, PondSimState.kGravity, mTime);
                Vector2 newPosition = mFallingEntities.Positions[i].Value + frogPositionDelta;
                //TODO: Deferred registry commands
                registry.SetComponent(mFallingEntities.Entities[i], new Velocity(newVelocity));
                registry.SetComponent(mFallingEntities.Entities[i], new Position(newPosition));
            }
        }
    }
}
