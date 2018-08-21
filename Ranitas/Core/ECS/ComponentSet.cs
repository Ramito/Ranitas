using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    internal class ComponentSet<TComponent> where TComponent : struct
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

        public bool HasComponent(Entity entity)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            uint entityIndex = entity.Index;
            uint sparseIndex = mSparseIndices[entityIndex];
            return (sparseIndex < Count) && (mPackedIndices[sparseIndex] == entityIndex);
        }

        public void AddComponent(Entity entity, TComponent component)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            Debug.Assert(!HasComponent(entity));
            mPackedComponents[Count] = component;
            uint entityIndex = entity.Index;
            mPackedIndices[Count] = entityIndex;
            mSparseIndices[entityIndex] = Count;
            ++Count;
        }

        public void SetComponent(Entity entity, TComponent component)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            Debug.Assert(HasComponent(entity));
            uint entityIndex = entity.Index;
            uint sparseIndex = mSparseIndices[entityIndex];
            mPackedComponents[sparseIndex] = component;
        }

        public void SetOrAddComponent(Entity entity, TComponent component)
        {
            if (HasComponent(entity))
            {
                SetComponent(entity, component);
            }
            else
            {
                AddComponent(entity, component);
            }
        }

        public TComponent GetComponent(Entity entity)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            Debug.Assert(HasComponent(entity));
            uint entityIndex = entity.Index;
            return mPackedComponents[mSparseIndices[entityIndex]];
        }

        public void RemoveComponent(Entity entity)
        {
            //The registry needs to ensure that no invalid entity gets here and that all invalid entities are cleared!
            Debug.Assert(HasComponent(entity));
            uint entityIndex = entity.Index;
            --Count;
            mPackedComponents[entityIndex] = mPackedComponents[Count];
            uint movedIndex = mPackedIndices[Count];
            mPackedIndices[entityIndex] = movedIndex;
            mSparseIndices[movedIndex] = entityIndex;
        }
    }
}
