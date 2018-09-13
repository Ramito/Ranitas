using Microsoft.Xna.Framework;
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

            //Swim data
            FrogSwimData swimData = new FrogSwimData(mFrogData.SwimKickDuration, mFrogData.SwimKickRecharge, mFrogData.SwimKickVelocity, mFrogData.WaterDrag, mFrogData.FrogDensity);
            mRegistry.AddComponent(frogEntity, swimData);

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

            //Velocity
            Velocity velocity = new Velocity();
            mRegistry.AddComponent(frogEntity, velocity);

            //Shape
            RectShape rectShape = new RectShape(mFrogData.Width, mFrogData.Height);
            mRegistry.AddComponent(frogEntity, rectShape);

            //Airborne - Spawned in the air!
            Airborne airborne = new Airborne();
            mRegistry.AddComponent(frogEntity, airborne);

            //Color
            Color color = Color.YellowGreen;
            mRegistry.AddComponent(frogEntity, color);

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
