using Microsoft.Xna.Framework.Graphics;
using Ranitas.Core;
using Ranitas.Core.ECS;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class ECSSim  //Working name only!
    {
        private const int kMaxEntities = 200;
        private EntityRegistry mRegistry = new EntityRegistry(kMaxEntities);
        private EventSystem mEventSystem = new EventSystem();

        private PlayerFactory mFactory;
        private List<ISystem> mSystems;
        private Render.RenderSystem mRenderSystem;  //TODO: Where to best place this guy?

        public ECSSim(RanitasDependencies dependencies)
        {
            mFactory = new PlayerFactory(mRegistry, dependencies.FrogData, dependencies.PondData);
            mSystems = RanitasSystems.MakeSystems(dependencies);
            mRenderSystem = new Render.RenderSystem(dependencies.GraphicsDevice, dependencies.PondState, dependencies.FrogSprite);
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
        public RanitasDependencies(float deltaTime, Data.PondData pondData, Data.FrogData frogData, Data.FlyData flyData, Texture2D frogSprite, GraphicsDevice graphicsDevice)
        {
            Time = new FrameTime(deltaTime);
            FrogData = frogData;
            PondData = pondData;
            FlyData = flyData;
            PondState = new Pond.PondSimState(pondData);
            GraphicsDevice = graphicsDevice;
            FrogSprite = frogSprite;
        }

        public readonly FrameTime Time;
        public readonly Data.FrogData FrogData;
        public readonly Data.PondData PondData;
        public readonly Data.FlyData FlyData;
        public readonly Pond.PondSimState PondState;
        public readonly GraphicsDevice GraphicsDevice; //TODO: Separate sim vs render dependencies?
        public readonly Texture2D FrogSprite;
    }

    public static class RanitasSystems
    {
        public static List<ISystem> MakeSystems(RanitasDependencies dependencies)
        {
            List<ISystem> systems = new List<ISystem>()
            {
                new FlySystem(dependencies.Time, dependencies.PondState, dependencies.FlyData),
                new FrogInputSystem(dependencies.Time, dependencies.FrogData),
                new FrogShapeDeformationSystem(dependencies.FrogData),
                new GravityPhysicsSystem(dependencies.Time),
                new SwimingFrogPhysicsSystem(dependencies.Time, dependencies.PondState, dependencies.FrogData),
                new WetDryFrogSystem(dependencies.PondState),
                new MainRectUpkeepSystem(),
                new LilyCollisionSystem(dependencies.PondState),
                new ToungueSystem(dependencies.FrogData, dependencies.Time),
                new ToungueShapeSystem(dependencies.FrogData),
                new TounguePositionSystem(dependencies.FrogData),
                new ParentedRectUpkeepSystem(),
                new InsectEatingSystem(),
            };
            return systems;
        }
    }
}
