using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    public interface IUntypedIndexedSet : IIndexDirectory
    {
        void Remove(uint atIndex);
    }

    public class IndexedSet<TValue> : IIndexDirectory, IUntypedIndexedSet where TValue : struct
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
}
