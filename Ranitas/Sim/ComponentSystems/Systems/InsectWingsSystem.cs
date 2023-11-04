using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Data;
using Ranitas.Sim.Events;
using System;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public struct InsectWingComponent
    {
        public Vector2 InitialPosition;
        public int Direction;
        public float TimeSpawned;
    }

    public sealed class InsectWingsSystem : ISystem
    {
        private EntityRegistry mRegistry;
        private FrameTime mTime;
        private FlyData mFlyData;
        private List<AteInsect> mAteEvents = new List<AteInsect>();
        private List<Entity> mRemovalList = new List<Entity>();

        private struct WingSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<InsectWingComponent> Wing;
        }
        private WingSlice mWingSlice;

        public InsectWingsSystem(RanitasDependencies dependencies)
        {
            mTime = dependencies.Time;
            mFlyData = dependencies.FlyData;
        }

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            mRegistry = registry;
            registry.SetupSlice(ref mWingSlice);
            eventSystem.AddMessageReceiver<AteInsect>(OnInsectEaten);
        }

        private void OnInsectEaten(AteInsect ateInsect)
        {
            mAteEvents.Add(ateInsect);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            foreach (AteInsect ateEvent in mAteEvents)
            {
                SpawnWings(ateEvent.InsectPosition);
            }
            mAteEvents.Clear();

            int count = mWingSlice.Entity.Count;
            for (int i = 0; i < count; ++i)
            {
                InsectWingComponent currentWing = mWingSlice.Wing[i];
                float totalTime = mTime.CurrentGameTime - currentWing.TimeSpawned;
                const float fallSpeed = -10f;
                const float rotateSpeed = 4f;
                const float radius = 12f;

                float rotation = rotateSpeed * totalTime;
                float cos = (float)Math.Cos(rotation);
                float fallDistance = fallSpeed * (rotation + cos - 1f);
                Vector2 offset = new Vector2(-currentWing.Direction * radius, fallDistance);
                Vector2 rotated = new Vector2(currentWing.Direction * cos, -(float)Math.Sin(rotation));
                Position newPosition = new Position
                {
                    Value = currentWing.InitialPosition + offset + radius * rotated,
                };
                mRegistry.SetComponent(mWingSlice.Entity[i], newPosition);

                if (newPosition.Value.Y < 0f)
                {
                    mRemovalList.Add(mWingSlice.Entity[i]);
                }
            }

            foreach (Entity entity in mRemovalList)
            {
                mRegistry.Destroy(entity);
            }
            mRemovalList.Clear();
        }

        private void SpawnWings(Vector2 position)
        {
            Entity wing1 = mRegistry.Create();
            Entity wing2 = mRegistry.Create();

            mRegistry.AddComponent(wing1, new InsectWingComponent
            {
                InitialPosition = position,
                Direction = 1,
                TimeSpawned = mTime.CurrentGameTime,
            });

            mRegistry.AddComponent(wing2, new InsectWingComponent
            {
                InitialPosition = position,
                Direction = -1,
                TimeSpawned = mTime.CurrentGameTime,
            });

            Position positionComponent = new Position(position);
            mRegistry.AddComponent(wing1, positionComponent);
            mRegistry.AddComponent(wing2, positionComponent);

            RectShape flyRect = new RectShape(mFlyData.Width, mFlyData.Height);
            mRegistry.AddComponent(wing1, flyRect);
            mRegistry.AddComponent(wing2, flyRect);

            mRegistry.AddComponent(wing1, Color.White);
            mRegistry.AddComponent(wing2, Color.White);
        }
    }
}
