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
        private TComponent[] mSparseComponents;
        private uint[] mSparseIndices;
        private uint[] mPackedIndices;

        public ComponentSet(int maxEntities)
        {
            mSparseComponents = new TComponent[maxEntities];
            mSparseIndices = new uint[maxEntities];
            mPackedIndices = new uint[maxEntities];
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
            mSparseComponents[entityIndex] = component;
            mSparseIndices[entityIndex] = Count;
            mPackedIndices[Count] = entityIndex;
            ++Count;
        }

        public void SetComponent(uint entityIndex, TComponent component)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            Debug.Assert(HasComponent(entityIndex));
            mSparseComponents[entityIndex] = component;
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
            return mSparseComponents[entityIndex];
        }

        public void RemoveComponent(uint entityIndex)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            Debug.Assert(HasComponent(entityIndex));
            --Count;
            uint deletedInPacked = mSparseIndices[entityIndex];
            uint movedIndex = mPackedIndices[Count];
            mSparseIndices[movedIndex] = deletedInPacked;
            mPackedIndices[deletedInPacked] = movedIndex;
        }
    }
}
