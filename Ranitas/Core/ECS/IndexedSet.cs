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

        public void Add(TValue value, int indexID)
        {
            mPackedValues[mIndexSet.Count] = value;
            mIndexSet.Add(indexID);
        }

        public bool Contains(int indexID)
        {
            return mIndexSet.Contains(indexID);
        }

        public void Replace(TValue newValue, int indexID)
        {
            int packedIndex = GetPackedIndex(indexID);
            mPackedValues[packedIndex] = newValue;
        }

        public TValue GetValue(int indexID)
        {
            int packedIndex = GetPackedIndex(indexID);
            return mPackedValues[packedIndex];
        }

        public void Remove(int indexID)
        {
            int packedIndex = GetPackedIndex(indexID);
            mIndexSet.Remove(indexID);
            mPackedValues[packedIndex] = mPackedValues[mIndexSet.Count];
        }

        public int GetPackedIndex(int indexID)
        {
            return mIndexSet.GetPackedIndex(indexID);
        }
    }
}
