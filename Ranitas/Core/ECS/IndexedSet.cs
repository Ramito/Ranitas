namespace Ranitas.Core.ECS
{
    public class IndexedSet<TValue> : IIndexedSet<TValue> where TValue : struct
    {
        private TValue[] mPackedValues;
        private IndexSet mIndexSet;

        public IndexedSet(int capacity)
        {
            mPackedValues = new TValue[capacity];
            mIndexSet = new IndexSet(capacity);
        }

        public void Add(TValue value, uint indexID)
        {
            mPackedValues[mIndexSet.Count] = value;
            mIndexSet.Add(indexID);
        }

        public bool Contains(uint indexID)
        {
            return mIndexSet.Contains(indexID);
        }

        public void Replace(TValue newValue, uint indexID)
        {
            uint packedIndex = GetPackedIndex(indexID);
            mPackedValues[packedIndex] = newValue;
        }

        public void AddOrReplace(TValue value, uint indexID)
        {
            if (Contains(indexID))
            {
                Replace(value, indexID);
            }
            else
            {
                Add(value, indexID);
            }
        }

        public TValue GetValue(uint indexID)
        {
            uint packedIndex = GetPackedIndex(indexID);
            return mPackedValues[packedIndex];
        }

        public void Remove(uint indexID)
        {
            uint packedIndex = GetPackedIndex(indexID);
            mIndexSet.Remove(indexID);
            mPackedValues[packedIndex] = mPackedValues[mIndexSet.Count];
        }

        public uint GetPackedIndex(uint indexID)
        {
            return mIndexSet.GetPackedIndex(indexID);
        }
    }

    public interface IUntypedComponentSet : IPublishingIndexSet, IUntypedIndexedSet
    { }

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
