using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    internal interface IUntypedComponentRequirement
    {
        bool MeetsRequirement(Entity entity);

        void StoreDataAtIndex(Entity dataFrom, int storeIndex);
    }

    internal class ComponentRequirement<TComponent> : IUntypedComponentRequirement where TComponent : struct
    {
        public bool MeetsRequirement(Entity entity)
        {
            return ComponentSet.HasComponent(entity.Index);
        }

        public void StoreDataAtIndex(Entity dataFrom, int storeIndex)
        {
            Debug.Assert(MeetsRequirement(dataFrom));
            TComponent data = ComponentSet.GetComponent(dataFrom.Index);
            SlicedComponents.Insert(storeIndex, data);
        }

        public TComponent FetchData(int index)
        {
            return SlicedComponents[index];
        }

        private ComponentSet<TComponent> ComponentSet;
        private List<TComponent> SlicedComponents;
    }

    public class EntitySlice
    {
        private int mCount = 0;
        private Dictionary<Type, int> mRequiredTypeMap = new Dictionary<Type, int>();
        private List<IUntypedComponentRequirement> mRequiredComponents = new List<IUntypedComponentRequirement>();
        private List<IUntypedComponentSet> mProhibitedComponents = new List<IUntypedComponentSet>();

        internal void RequireComponent<TComponent>(ComponentRequirement<TComponent> requirement) where TComponent : struct
        {
            Type componentType = typeof(TComponent);
            Debug.Assert(!mRequiredTypeMap.ContainsKey(componentType));
            mRequiredTypeMap[componentType] = mRequiredComponents.Count;
            mRequiredComponents.Add(requirement);
        }

        internal void ExcludeComponent<TComponent>(ComponentSet<TComponent> componentSet) where TComponent : struct
        {
            Type componentType = typeof(TComponent);
            Debug.Assert(!mRequiredTypeMap.ContainsKey(componentType));
            mRequiredTypeMap[componentType] = -1;
            mProhibitedComponents.Add(componentSet);
        }

        public void TryInsertEntity(Entity entity)
        {
            //Implementation does not support validating if the slice already has an entity!
            foreach (IUntypedComponentRequirement requirement in mRequiredComponents)
            {
                if (!requirement.MeetsRequirement(entity))
                {
                    return;
                }
            }
            foreach (IUntypedComponentSet componentSet in mProhibitedComponents)
            {
                if (componentSet.HasComponent(entity.Index))
                {
                    return;
                }
            }
            //Entity has all requirements!
            foreach (IUntypedComponentRequirement requirement in mRequiredComponents)
            {
                requirement.StoreDataAtIndex(entity, mCount);
            }
            ++mCount;
        }

        public TComponent GetComponent<TComponent>(int i) where TComponent : struct
        {
            Type componentType = typeof(TComponent);
            Debug.Assert(mRequiredTypeMap.ContainsKey(componentType));
            int index = mRequiredTypeMap[componentType];
            Debug.Assert(index > 0);
            ComponentRequirement<TComponent> requirement = (ComponentRequirement<TComponent>)mRequiredComponents[index];
            return requirement.FetchData(i);
        }
    }
}
