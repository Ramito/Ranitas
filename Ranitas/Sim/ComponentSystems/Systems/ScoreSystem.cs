using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Sim.Events;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class ScoreSystem : ISystem
    {
        public ScoreSystem(int playerCount)
        {
            mScoreBuffer = new List<Entity>(playerCount);
        }

        private struct NoScorePlayersSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirement<Player> IsPlayer;
            public SliceExclusion<Score> NoScore;
        }
        private NoScorePlayersSlice mNoScoreSlice;

        private struct ScoreSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<ControlledEntity> ControlledEntity;
            public SliceRequirementOutput<Score> Score;
        }
        private ScoreSlice mScoreSlice;

        private List<Entity> mScoreBuffer;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mNoScoreSlice);
            registry.SetupSlice(ref mScoreSlice);
            eventSystem.AddMessageReceiver<AteInsect>(OnFlyEaten);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int noScoreCount = mNoScoreSlice.Entity.Count;
            for (int i = noScoreCount - 1; i >= 0; --i)
            {
                registry.AddComponent(mNoScoreSlice.Entity[i], new Score());
            }

            if (mScoreBuffer.Count > 0)
            {
                int playerCount = mScoreSlice.Entity.Count;
                for (int i = 0; i < playerCount; ++i)
                {
                    int currentScore = mScoreSlice.Score[i].Value;
                    Entity controlledFrog = mScoreSlice.ControlledEntity[i].Entity;
                    foreach (Entity frog in mScoreBuffer)
                    {
                        if (frog == controlledFrog)
                        {
                            ++currentScore;
                        }
                    }
                    registry.SetComponent(mScoreSlice.Entity[i], new Score(currentScore));
                }
                mScoreBuffer.Clear();
            }
        }

        private void OnFlyEaten(AteInsect ateInsect)
        {
            mScoreBuffer.Add(ateInsect.EatenBy);
        }
    }
}
