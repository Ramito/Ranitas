using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Data;

namespace Ranitas.Sim
{
    public sealed class FrogFactory
    {
        public FrogFactory(FrogData frogData, PondData pondData)
        {
            mFrogData = frogData;
            mPondData = pondData;
        }

        private readonly FrogData mFrogData;
        private readonly PondData mPondData;

        public Entity MakeFrog(int index, EntityRegistry registry)
        {
            Entity frogEntity = registry.Create();

            //Controller state
            FrogControlState controlState = new FrogControlState();
            registry.AddComponent(frogEntity, controlState);

            //Position Component
            int spawnIndex = index % mPondData.FrogSpawns.Length;
            float spawnX = mPondData.FrogSpawns[spawnIndex];
            float spawnY = mPondData.Height + mFrogData.Height;
            Vector2 positionValue = new Vector2(spawnX, spawnY);
            Position spawnPosition = new Position(positionValue);
            registry.AddComponent(frogEntity, spawnPosition);

            //Facing
            Facing facing = new Facing();
            registry.AddComponent(frogEntity, facing);

            //Velocity
            Velocity velocity = new Velocity();
            registry.AddComponent(frogEntity, velocity);

            //Shape
            RectShape rectShape = new RectShape(mFrogData.Width, mFrogData.Height);
            registry.AddComponent(frogEntity, rectShape);

            //Rect - Added automatically by the RectUpkeepSystem, but I don't want to worry about the first frame this being missing
            Rect rect = new Rect(spawnPosition.Value, rectShape.Width, rectShape.Height);

            //Gravity - Spawned in the air!
            Gravity gravity = new Gravity();
            registry.AddComponent(frogEntity, gravity);

            //Animation
            AnimationState animation = new AnimationState();
            registry.AddComponent(frogEntity, animation);

            return frogEntity;
        }
    }
}
