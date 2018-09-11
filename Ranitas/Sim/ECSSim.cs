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
        public readonly FrameTime Time;
        public readonly Pond.PondSimState Pond;
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
                new FrogInputSystem(),
                new WetDryFrogSystem(dependencies.Pond),
                new FrogPhysicsSystem(dependencies.Time, dependencies.Pond),
            };
            return systems;
        }
    }
}
