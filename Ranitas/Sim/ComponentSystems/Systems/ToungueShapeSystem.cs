using Ranitas.Core;
using Ranitas.Core.ECS;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class ToungueShapeSystem : ISystem
    {
        public ToungueShapeSystem(Data.FrogData data)
        {
            mToungueData = new ToungueData(data);
            mShapeData = new ToungueShapeData(data);
            mDoneToungues = new List<Entity>();
        }

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mTounguesWithShape);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int count = mTounguesWithShape.Entity.Count;
            for (int i = 0; i < count; ++i)
            {
                ToungueState currentState = mTounguesWithShape.ToungueState[i];
                switch(currentState.Stage)
                {
                    case ToungueStages.Refreshing:
                    default:
                        mDoneToungues.Add(mTounguesWithShape.Entity[i]);
                        break;
                    case ToungueStages.Retracting:
                        float relativeStage = currentState.TimeLeft / mToungueData.GetStateTime(ToungueStages.Retracting);
                        float width = mShapeData.Length * relativeStage;
                        registry.SetComponent(mTounguesWithShape.Entity[i], new RectShape(width, mShapeData.Thickness));
                        break;
                    case ToungueStages.Extended:
                        registry.SetComponent(mTounguesWithShape.Entity[i], new RectShape(mShapeData.Length, mShapeData.Thickness));
                        break;
                    case ToungueStages.Extending:
                        relativeStage = currentState.TimeLeft / mToungueData.GetStateTime(ToungueStages.Extending);
                        width = mShapeData.Length * (1f - relativeStage);
                        registry.SetComponent(mTounguesWithShape.Entity[i], new RectShape(width, mShapeData.Thickness));
                        break;
                }
            }
            foreach (Entity toungue in mDoneToungues)
            {
                registry.RemoveComponent<RectShape>(toungue);
                registry.RemoveComponent<Position>(toungue);    //Weird to remove here?
            }
            mDoneToungues.Clear();
        }

        private struct TounguesWithShape
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<ToungueState> ToungueState;
            public SliceRequirement<RectShape> HasShape;
        }
        private TounguesWithShape mTounguesWithShape;

        private ToungueData mToungueData;
        private ToungueShapeData mShapeData;
        private List<Entity> mDoneToungues;
    }
}
