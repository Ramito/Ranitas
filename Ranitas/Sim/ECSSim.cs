﻿using Microsoft.Xna.Framework.Graphics;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Render;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class ECSSim  //Working name only!
    {
        private const int kMaxEntities = 200;
        private EntityRegistry mRegistry = new EntityRegistry(kMaxEntities);
        private EventSystem mEventSystem = new EventSystem();
        private FrameTime mTime;

        private PlayerFactory mFactory;
        private List<ISystem> mSystems;
        private List<ISystem> mRenderingSystems;

        public ECSSim(RanitasDependencies dependencies)
        {
            FrogFactory frogFactory = new FrogFactory(dependencies.FrogData, dependencies.PondData);
            mFactory = new PlayerFactory(frogFactory);
            mSystems = RanitasSystems.MakeSimSystems(dependencies);
            mRenderingSystems = RanitasSystems.MakeRenderSystems(dependencies);
            mTime = dependencies.Time;
        }

        public void SpawnPlayer(int index)
        {
            mFactory.MakePlayer(index, mRegistry);
        }

        public void Initialize()
        {
            mFactory.Initialize(mRegistry);
            foreach (ISystem system in mSystems)
            {
                system.Initialize(mRegistry, mEventSystem);
            }
            foreach (ISystem system in mRenderingSystems)
            {
                system.Initialize(mRegistry, mEventSystem);
            }
        }

        public void Update()
        {
            mTime.CurrentGameTime += mTime.DeltaTime;
            foreach (ISystem system in mSystems)
            {
                system.Update(mRegistry, mEventSystem);
            }
        }

        public void Render()
        {
            foreach (ISystem system in mRenderingSystems)
            {
                system.Update(mRegistry, mEventSystem);
            }
        }
    }

    public class RanitasDependencies
    {
        public RanitasDependencies(float deltaTime, Data.PondData pondData, Data.FrogData frogData, Data.FlyData flyData, Data.FlyDirectionChangeData directionChangeData, Data.FlyNoiseData flyNoiseData, Data.FrogAnimationData animationData, Texture2D frogSprite, GraphicsDevice graphicsDevice, SpriteFont uiFont, Effect waterEffect)
        {
            Time = new FrameTime(deltaTime);
            FrogData = frogData;
            PondData = pondData;
            FlyData = flyData;
            FlyNoiseData = flyNoiseData;
            DirectionChangeData = directionChangeData;
            AnimationData = animationData;
            PondState = new Pond.PondSimState(pondData);
            GraphicsDevice = graphicsDevice;
            FrogSprite = frogSprite;
            UIFont = uiFont;
            WaterEffect = waterEffect;
        }

        public readonly FrameTime Time;
        public readonly Data.FrogData FrogData;
        public readonly Data.PondData PondData;
        public readonly Data.FlyData FlyData;
        public readonly Data.FlyNoiseData FlyNoiseData;
        public readonly Data.FlyDirectionChangeData DirectionChangeData;
        public readonly Data.FrogAnimationData AnimationData;
        public readonly Pond.PondSimState PondState;
        public readonly GraphicsDevice GraphicsDevice; //TODO: Separate sim vs render dependencies?
        public readonly Texture2D FrogSprite;
        public readonly SpriteFont UIFont;
        public readonly Effect WaterEffect;
    }

    public static class RanitasSystems
    {
        public static List<ISystem> MakeSimSystems(RanitasDependencies dependencies)
        {
            List<ISystem> systems = new List<ISystem>()
            {
                new FlySpawnSystem(dependencies.Time, dependencies.PondState, dependencies.FlyData),
                new FlyDirectionSystem(dependencies.Time, dependencies.PondState, dependencies.FlyData, dependencies.DirectionChangeData),
                new FlyMoveSystem(dependencies.Time, dependencies.FlyData),
                new FlyNoiseSystem(dependencies.Time, dependencies.FlyData, dependencies.FlyNoiseData),
                new FrogInputSystem(dependencies.Time, dependencies.FrogData),
                new FrogShapeDeformationSystem(dependencies.FrogData),
                new GravityPhysicsSystem(dependencies.Time),
                new SwimingFrogPhysicsSystem(dependencies.Time, dependencies.PondState, dependencies.FrogData),
                new WetDryFrogSystem(dependencies.PondState),
                new MainRectUpkeepSystem(),
                new LilyCollisionSystem(dependencies.PondState),
                new WaterSystem(dependencies.Time, dependencies.PondState),
                new ToungueSystem(dependencies.FrogData, dependencies.Time),
                new ToungueShapeSystem(dependencies.FrogData),
                new TounguePositionSystem(dependencies.FrogData),
                new ParentedRectUpkeepSystem(),
                new InsectEatingSystem(),
                new InsectWingsSystem(dependencies),
                new ScoreSystem(4),
            };
            return systems;
        }

        public static List<ISystem> MakeRenderSystems(RanitasDependencies dependencies)
        {
            List<ISystem> systems = new List<ISystem>()
            {
                new FlyAnimationSystem(dependencies),
                new FrogAnimationSystem(dependencies.AnimationData),
                new RenderSystem(dependencies.GraphicsDevice, dependencies.PondState, dependencies.FrogSprite, dependencies.UIFont, dependencies.AnimationData, dependencies.WaterEffect, dependencies.Time),
            };
            return systems;
        }
    }
}
