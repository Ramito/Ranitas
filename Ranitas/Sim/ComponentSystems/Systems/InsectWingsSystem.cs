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
        public float FallSpeed;
        public float TranslateSpeed;
        public float RotateSpeed;
        public float HRadius;
        public float VRadius;
    }

    public sealed class InsectWingsSystem : ISystem
    {
        private EntityRegistry mRegistry;
        private FrameTime mTime;
        private FlyData mFlyData;
        private List<AteInsect> mAteEvents = new List<AteInsect>();
        private List<Entity> mRemovalList = new List<Entity>();
        private Random mRandom = new Random();

        private const float FallSpeed = -5f;
        private const float TranslateSpeed = 2.5f;
        private const float RotateSpeed = 3f;
        private const float HRadius = 8f;
        private const float VRadius = 3f;

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

                float rotation = currentWing.RotateSpeed * totalTime;
                float cos = (float)Math.Cos(rotation);
                float sin = (float)Math.Sin(rotation);
                float vertical = currentWing.FallSpeed * (rotation + currentWing.VRadius * (Math.Abs(cos) - 1f));
                float horizontal = currentWing.Direction * currentWing.TranslateSpeed * (rotation + currentWing.HRadius * sin);

                Position newPosition = new Position
                {
                    Value = currentWing.InitialPosition + new Vector2(horizontal, vertical),
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

        private float RandomOffset()
        {
            return (float)(2.0 * mRandom.NextDouble() - 1.0);
        }

        private InsectWingComponent MakeWingComponent(Vector2 position, int direction)
        {
            return new InsectWingComponent
            {
                InitialPosition = position,
                Direction = direction,
                TimeSpawned = mTime.CurrentGameTime,
                FallSpeed = FallSpeed + 0.1f * RandomOffset(),
                TranslateSpeed = TranslateSpeed + 0.5f * RandomOffset(),
                RotateSpeed = RotateSpeed + 0.25f * RandomOffset(),
                HRadius = HRadius + 0.2f * RandomOffset(),
                VRadius = VRadius + 0.2f * RandomOffset(),
            };
        }

        private void SpawnWings(Vector2 position)
        {
            Entity wing1 = mRegistry.Create();
            Entity wing2 = mRegistry.Create();

            mRegistry.AddComponent(wing1, MakeWingComponent(position, 1));

            mRegistry.AddComponent(wing2, MakeWingComponent(position, -1));

            Position positionComponent = new Position(position);
            mRegistry.AddComponent(wing1, positionComponent);
            mRegistry.AddComponent(wing2, positionComponent);

            RectShape flyRect = new RectShape(mFlyData.Width * mFlyData.WingSize, mFlyData.Height * mFlyData.WingSize);
            mRegistry.AddComponent(wing1, flyRect);
            mRegistry.AddComponent(wing2, flyRect);

            mRegistry.AddComponent(wing1, Color.LightGray);
            mRegistry.AddComponent(wing2, Color.LightGray);
        }
    }
}
