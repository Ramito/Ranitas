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
        private PlayerFactory mFactory;

        public ECSSim(RanitasDependencies dependencies)
        {
            mSystems = RanitasSystems.MakeSystems(dependencies);
            mFactory = new PlayerFactory(mRegistry, dependencies.FrogData, dependencies.PondData);
        }

        public void SpawnPlayer(int index)
        {
            mFactory.MakePlayer(index);
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
        public RanitasDependencies(float deltaTime, Data.PondData pondData, Data.FrogData frogData)
        {
            Time = new FrameTime(deltaTime);
            FrogData = frogData;
            PondData = pondData;
            PondState = new Pond.PondSimState(pondData);
        }

        public readonly FrameTime Time;
        public readonly Data.FrogData FrogData;
        public readonly Data.PondData PondData;
        public readonly Pond.PondSimState PondState;
    }

    public static class RanitasSystems
    {
        public static RanitasDependencies MakeDependencies(float deltaTime, Data.PondData pondData, Data.FrogData frogData)
        {
            RanitasDependencies dependencies = new RanitasDependencies(deltaTime, pondData, frogData);
            return dependencies;
        }

        public static List<ISystem> MakeSystems(RanitasDependencies dependencies)
        {
            List<ISystem> systems = new List<ISystem>()
            {
                new FrogInputSystem(dependencies.Time, dependencies.FrogData),
                new WetDryFrogSystem(dependencies.PondState),
                new FrogPhysicsSystem(dependencies.Time, dependencies.PondState),
            };
            return systems;
        }
    }
}
