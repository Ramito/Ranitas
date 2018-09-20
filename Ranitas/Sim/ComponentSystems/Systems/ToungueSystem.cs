using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class ToungueSystem : ISystem
    {
        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mNoToungueSlice);
            registry.SetupSlice(ref mAllToungues);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            UpdateNoToungueFrogs(registry);
            UpdateToungueState(registry);
        }

        public ToungueSystem(Data.FrogData data, FrameTime time)
        {
            mData = new ToungueData(data);
            mTime = time;
            mTransitioning = new List<Entity>();
        }

        private ToungueData mData;
        private FrameTime mTime;
        private List<Entity> mTransitioning;

        private void UpdateNoToungueFrogs(EntityRegistry registry)
        {
            int count = mNoToungueSlice.Entity.Count;
            for (int i = 0; i < count; ++i)
            {
                if (mNoToungueSlice.ControlState[i].ToungueSignalState)
                {
                    mTransitioning.Add(mNoToungueSlice.Entity[i]);
                }
            }
            foreach (Entity frog in mTransitioning)
            {
                MakeToungue(registry, frog);
            }
            mTransitioning.Clear();
        }

        private void UpdateToungueState(EntityRegistry registry)
        {
            int count = mAllToungues.Entity.Count;
            for (int i = 0; i < count; ++i)
            {
                ToungueState state = mAllToungues.State[i];
                float newTime = state.TimeLeft - mTime.DeltaTime;
                if (newTime > 0)
                {
                    state.TimeLeft = newTime;
                    registry.SetComponent(mAllToungues.Entity[i], state);
                }
                else
                {
                    int stateIndex = (int)state.Stage;
                    if (stateIndex > 0)
                    {
                        ToungueStages nextStage = (ToungueStages)(stateIndex - 1);
                        float nextTime = mData.GetStateTime(nextStage);
                        registry.SetComponent(mAllToungues.Entity[i], new ToungueState(nextStage, nextTime));
                    }
                    else
                    {
                        mTransitioning.Add(mAllToungues.Entity[i]);
                    }
                }
            }
            foreach (Entity toungue in mTransitioning)
            {
                Entity frog = registry.GetComponent<ParentEntity>(toungue).Parent;
                registry.RemoveComponent<ControlledEntity>(frog);
                registry.Destroy(toungue);
            }
            mTransitioning.Clear();
        }

        private void MakeToungue(EntityRegistry registry, Entity parentFrog)
        {
            Entity toungue = registry.Create();
            ToungueState state = new ToungueState(ToungueStages.Extending, mData.GetStateTime(ToungueStages.Extending));
            registry.AddComponent(toungue, state);
            registry.AddComponent(toungue, new RectShape());
            registry.AddComponent(toungue, new Position());
            registry.AddComponent(toungue, Color.Red);
            registry.AddComponent(toungue, new ParentEntity(parentFrog, new Vector2()));

            ControlledEntity controlledToungue = new ControlledEntity(toungue);
            registry.AddComponent(parentFrog, controlledToungue);
        }

        private struct NoToungueFrogsSlice
        {
            public SliceEntityOutput Entity;
            public SliceExclusion<ControlledEntity> NoToungue;
            public SliceExclusion<Waterborne> NotInWater;
            public SliceRequirementOutput<FrogControlState> ControlState;
        }
        private NoToungueFrogsSlice mNoToungueSlice;

        private struct AllTounguesSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<ToungueState> State;
        }
        private AllTounguesSlice mAllToungues;
    }
}
