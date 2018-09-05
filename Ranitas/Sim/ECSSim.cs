using Ranitas.Core.ECS;
using Ranitas.Core.EventSystem;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class ECSSim  //Working name only!
    {
        private const int kMaxEntities = 2000;
        private EntityRegistry mRegistry = new EntityRegistry(kMaxEntities);
        private EventSystem mEventSystem = new EventSystem();

        private List<ISystem> mSystems;

        public ECSSim()
        {
            RanitasDependencies dependencies = RanitasSystems.MakeDependencies();
            mSystems = RanitasSystems.MakeSystems(dependencies);
        }

        public void Initialize()
        {
            foreach (ISystem system in mSystems)
            {
                system.Initialize(mRegistry, mEventSystem);
            }
        }

        public void Update()
        {
            foreach (ISystem system in mSystems)
            {
                system.Update(mRegistry, mEventSystem);
            }
        }
    }

    public class RanitasDependencies
    {
        public Pond.PondSimState Pond;
    }

    public static class RanitasSystems
    {
        public static RanitasDependencies MakeDependencies()
        {
            RanitasDependencies dependencies = new RanitasDependencies();
            return dependencies;
        }

        public static List<ISystem> MakeSystems(RanitasDependencies dependencies)
        {
            List<ISystem> systems = new List<ISystem>()
            {
                new WetDryFrogSystem(dependencies.Pond),
            };
            return systems;
        }
    }


    public static class FrogFactory
    {
        public static Entity MakeFrog(EntityRegistry registry)
        {
            Entity entity = registry.Create();

            //WIP WE NEED SPAWN LOCATIONS
            registry.AddComponent(entity, new Airborne());

            return entity;
        }
    }
}
