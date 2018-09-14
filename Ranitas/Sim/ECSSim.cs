using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Core.Render;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class ECSSim  //Working name only!
    {
        private const int kMaxEntities = 2000;
        private EntityRegistry mRegistry = new EntityRegistry(kMaxEntities);
        private EventSystem mEventSystem = new EventSystem();

        private PlayerFactory mFactory;
        private List<ISystem> mSystems;
        private Render.RenderSystem mRenderSystem;  //TODO: Where to best place this guy?

        public ECSSim(RanitasDependencies dependencies)
        {
            mFactory = new PlayerFactory(mRegistry, dependencies.FrogData, dependencies.PondData);
            mSystems = RanitasSystems.MakeSystems(dependencies);
            mRenderSystem = new Render.RenderSystem(dependencies.Renderer, dependencies.PondState);
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
            mRenderSystem.Initialize(mRegistry, mEventSystem);
        }

        public void Update()
        {
            foreach (ISystem system in mSystems)
            {
                system.Update(mRegistry, mEventSystem);
            }
        }

        public void Render()
        {
            mRenderSystem.Update(mRegistry, mEventSystem);
        }
    }

    public class RanitasDependencies
    {
        public RanitasDependencies(float deltaTime, Data.PondData pondData, Data.FrogData frogData, PrimitiveRenderer renderer)
        {
            Time = new FrameTime(deltaTime);
            FrogData = frogData;
            PondData = pondData;
            PondState = new Pond.PondSimState(pondData);
            Renderer = renderer;
        }

        public readonly FrameTime Time;
        public readonly Data.FrogData FrogData;
        public readonly Data.PondData PondData;
        public readonly Pond.PondSimState PondState;
        public readonly PrimitiveRenderer Renderer; //TODO: Separate sim vs render dependencies?
    }

    public static class RanitasSystems
    {
        public static RanitasDependencies MakeDependencies(float deltaTime, Data.PondData pondData, Data.FrogData frogData, PrimitiveRenderer renderer)
        {
            RanitasDependencies dependencies = new RanitasDependencies(deltaTime, pondData, frogData, renderer);
            return dependencies;
        }

        public static List<ISystem> MakeSystems(RanitasDependencies dependencies)
        {
            List<ISystem> systems = new List<ISystem>()
            {
                new FrogInputSystem(dependencies.Time, dependencies.FrogData),
                new FrogPhysicsSystem(dependencies.Time, dependencies.PondState, dependencies.FrogData),
                new WetDryFrogSystem(dependencies.PondState),
            };
            return systems;
        }
    }
}
