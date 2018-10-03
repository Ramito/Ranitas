using Ranitas.Core.ECS;
using System.Diagnostics;

namespace Ranitas.Sim
{
    public sealed class PlayerFactory
    {
        public PlayerFactory(FrogFactory frogFactory)
        {
            mFrogFactory = frogFactory;
        }
        
        private readonly FrogFactory mFrogFactory;

        public void Initialize(EntityRegistry registry)
        {
            SetupDebugSlice(registry);
        }

        public Entity MakePlayer(int index, EntityRegistry registry)
        {
            AssertIndexIsNotUsed(index);

            Entity playerEntity = registry.Create();
            registry.AddComponent(playerEntity, new Player(index));

            Entity frogEntity = mFrogFactory.MakeFrog(index, registry);

            ControlledEntity controlledEntity = new ControlledEntity(frogEntity);
            registry.AddComponent(playerEntity, controlledEntity);

            return playerEntity;
        }

#if DEBUG
        private struct PlayersSlice
        {
            public SliceRequirementOutput<Player> Player;
        }
        private PlayersSlice mPlayersSlice;
#endif

        [Conditional("DEBUG")]
        private void SetupDebugSlice(EntityRegistry registry)
        {
#if DEBUG
            registry.SetupSlice(ref mPlayersSlice);
#endif
        }

        [Conditional("DEBUG")]
        private void AssertIndexIsNotUsed(int index)
        {
#if DEBUG
            int count = mPlayersSlice.Player.Count;
            for (int i = 0; i < count; ++i)
            {
                Debug.Assert(mPlayersSlice.Player[i].Index != index);
            }
#endif
        }
    }
}
