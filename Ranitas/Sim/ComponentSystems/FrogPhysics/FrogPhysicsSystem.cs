using Microsoft.Xna.Framework;
using Ranitas.Core.ECS;
using Ranitas.Core.EventSystem;
using Ranitas.Pond;

namespace Ranitas.Sim
{
    public sealed class FrogPhysicsSystem : ISystem
    {
        private float TimeStep;    //TODO: Hook up!
        private float HalfTimeStepSquared;
        private PondSimState mPond;

        private struct DryFrogs
        {
            public SliceEntityOutput Entities;
            public SliceExclusion<Waterborne> Dry;
            public SliceRequirementOutput<Position> Positions;
            public SliceRequirementOutput<Velocity> Velocities;
        }
        private DryFrogs mDryFrogs = new DryFrogs();

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mDryFrogs);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            //TODO: Frog shapes
            UpdateAirborneFrogs(registry);
            //TODO: Swiming frog!
        }

        private void UpdateAirborneFrogs(EntityRegistry registry)
        {
            int count = mDryFrogs.Entities.Count;
            for (int i = 0; i < count; ++i)
            {
                Vector2 velocityDelta = TimeStep * PondSimState.kGravity;
                Vector2 newVelocity = mDryFrogs.Velocities[i].Value + velocityDelta;
                Vector2 frogPositionDelta = FramePositionDelta(mDryFrogs.Velocities[i].Value);
                Vector2 newPosition = mDryFrogs.Positions[i].Value + frogPositionDelta;
                //TODO: Deferred registry commands?
                registry.SetComponent(mDryFrogs.Entities[i], new Velocity(newVelocity));
                registry.SetComponent(mDryFrogs.Entities[i], new Position(newPosition));
            }

        }

        private Vector2 FramePositionDelta(Vector2 frameVelocity)
        {
            Vector2 velocityContribution = TimeStep * frameVelocity;
            Vector2 accelerationContribution = HalfTimeStepSquared * PondSimState.kGravity;
            return velocityContribution + accelerationContribution;
        }
    }
}
