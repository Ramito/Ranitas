using Ranitas.Core.ECS;
using Ranitas.Core.EventSystem;

namespace Ranitas.Sim
{
    public sealed class ECSSim  //Working name only!
    {
        private const int kMaxEntities = 2000;
        private EntityRegistry mRegistry = new EntityRegistry(kMaxEntities);
        private EventSystem mEventSystem = new EventSystem();

        //WIP ControlUpdater should be ported to this!

        //Determine if frog is landed
        //Determine if frog is airborne
        //Update airborne frogs
        //Update swimming frogs
        //Update flies
        //Update toungues
        //Check flies vs toungues

    }

    public static class FrogFactory
    {
        public static Entity MakeFrog(EntityRegistry registry)
        {
            Entity entity = registry.Create();

            registry.AddComponent(entity, new WetDryFrogState());

            return entity;
        }
    }
}
