using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    internal interface IUntypedComponentSet
    {
        bool HasComponent(uint entityIndex);
        void RemoveComponent(uint entityIndex);
    }

    internal class ComponentSet<TComponent> : IUntypedComponentSet where TComponent : struct
    {
        private uint Count = 0;
        private TComponent[] mPackedComponents;
        private uint[] mPackedIndices;
        private uint[] mSparseIndices;

        public ComponentSet(int maxEntities)
        {
            mPackedComponents = new TComponent[maxEntities];
            mPackedIndices = new uint[maxEntities];
            mSparseIndices = new uint[maxEntities];
        }

        public bool HasComponent(uint entityIndex)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            uint sparseIndex = mSparseIndices[entityIndex];
            return (sparseIndex < Count) && (mPackedIndices[sparseIndex] == entityIndex);
        }

        public void AddComponent(uint entityIndex, TComponent component)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            Debug.Assert(!HasComponent(entityIndex));
            mPackedComponents[Count] = component;
            mPackedIndices[Count] = entityIndex;
            mSparseIndices[entityIndex] = Count;
            ++Count;
        }

        public void SetComponent(uint entityIndex, TComponent component)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            Debug.Assert(HasComponent(entityIndex));
            uint sparseIndex = mSparseIndices[entityIndex];
            mPackedComponents[sparseIndex] = component;
        }

        public void SetOrAddComponent(uint entityIndex, TComponent component)
        {
            if (HasComponent(entityIndex))
            {
                SetComponent(entityIndex, component);
            }
            else
            {
                AddComponent(entityIndex, component);
            }
        }

        public TComponent GetComponent(uint entityIndex)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            Debug.Assert(HasComponent(entityIndex));
            return mPackedComponents[mSparseIndices[entityIndex]];
        }

        public void RemoveComponent(uint entityIndex)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            Debug.Assert(HasComponent(entityIndex));
            --Count;
            uint deletedInPacked = mSparseIndices[entityIndex];
            mPackedComponents[deletedInPacked] = mPackedComponents[Count];
            uint movedIndex = mPackedIndices[Count];
            mPackedIndices[deletedInPacked] = movedIndex;
            mSparseIndices[movedIndex] = deletedInPacked;
        }
    }
}
