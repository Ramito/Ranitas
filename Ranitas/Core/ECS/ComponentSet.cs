namespace Ranitas.Core.ECS
{
    public class ComponentSet<TValue> : IUntypedComponentSet, IIndexedSet<TValue> where TValue : struct
    {
        public ComponentSet(int capacity)
        {
            mIndexedSet = new IndexedSet<TValue>(capacity);
        }

        private readonly IndexedSet<TValue> mIndexedSet;

        public event IndexedValueHandler NewValue;
        public event IndexedValueHandler ValueModified;
        public event IndexedValueHandler Removed;

        public void Add(TValue value, uint indexID)
        {
            mIndexedSet.Add(value, indexID);
            NewValue?.Invoke(indexID);
        }

        public void AddOrReplace(TValue value, uint indexID)
        {
            mIndexedSet.AddOrReplace(value, indexID);
        }

        public bool Contains(uint indexID)
        {
            return mIndexedSet.Contains(indexID);
        }

        public uint GetPackedIndex(uint indexID)
        {
            return mIndexedSet.GetPackedIndex(indexID);
        }

        public TValue GetValue(uint indexID)
        {
            return mIndexedSet.GetValue(indexID);
        }

        public void Remove(uint atIndex)
        {
            Removed?.Invoke(atIndex);
            mIndexedSet.Remove(atIndex);
        }

        public void Replace(TValue newValue, uint indexID)
        {
            mIndexedSet.Replace(newValue, indexID);
            ValueModified?.Invoke(indexID);
        }
    }
}
