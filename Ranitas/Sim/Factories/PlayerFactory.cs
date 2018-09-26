using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Data;
using System.Diagnostics;

namespace Ranitas.Sim
{
    public class PlayerFactory
    {
        public PlayerFactory(EntityRegistry registry, FrogData frogData, PondData pondData)
        {
            mRegistry = registry;
            mFrogData = frogData;
            mPondData = pondData;

            mRegistry.SetupSlice(ref mPlayersSlice);
        }

        private struct PlayersSlice
        {
            public SliceRequirementOutput<Player> Player;
        }
        private PlayersSlice mPlayersSlice;

        private readonly EntityRegistry mRegistry;
        private readonly FrogData mFrogData;
        private readonly PondData mPondData;

        public Entity MakePlayer(int index)
        {
            AssertIndexIsNotUsed(index);

            Entity playerEntity = mRegistry.Create();
            mRegistry.AddComponent(playerEntity, new Player(index));

            Entity frogEntity = MakeFrog(index);

            ControlledEntity controlledEntity = new ControlledEntity(frogEntity);
            mRegistry.AddComponent(playerEntity, controlledEntity);

            return playerEntity;
        }

        private Entity MakeFrog(int index)
        {
            Entity frogEntity = mRegistry.Create();

            //Controller state
            FrogControlState controlState = new FrogControlState();
            mRegistry.AddComponent(frogEntity, controlState);

            //Position Component
            int spawnIndex = index % mPondData.FrogSpawns.Length;
            float spawnX = mPondData.FrogSpawns[spawnIndex];
            float spawnY = mPondData.Height + mFrogData.Height;
            Vector2 positionValue = new Vector2(spawnX, spawnY);
            Position spawnPosition = new Position(positionValue);
            mRegistry.AddComponent(frogEntity, spawnPosition);

            //Facing
            Facing facing = new Facing();
            mRegistry.AddComponent(frogEntity, facing);

            //Velocity
            Velocity velocity = new Velocity();
            mRegistry.AddComponent(frogEntity, velocity);

            //Shape
            RectShape rectShape = new RectShape(mFrogData.Width, mFrogData.Height);
            mRegistry.AddComponent(frogEntity, rectShape);

            //Rect - Added automatically by the RectUpkeepSystem, but I don't want to worry about the first frame this being missing
            Rect rect = new Rect(spawnPosition.Value, rectShape.Width, rectShape.Height);

            //Gravity - Spawned in the air!
            Gravity gravity = new Gravity();
            mRegistry.AddComponent(frogEntity, gravity);

            //Animation
            AnimationState animation = new AnimationState();
            mRegistry.AddComponent(frogEntity, animation);

            return frogEntity;
        }

        [Conditional("Debug")]
        private void AssertIndexIsNotUsed(int index)
        {
            int count = mPlayersSlice.Player.Count;
            for (int i = 0; i < count; ++i)
            {
                Debug.Assert(mPlayersSlice.Player[i].Index != index);
            }
        }
    }
}
