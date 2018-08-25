using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    public interface IUntypedIndexedSet
    {
        bool Contains(uint index);
        void Remove(uint atIndex);
    }

    public class IndexedSet<TValue> : IUntypedIndexedSet where TValue : struct
    {
        private TValue[] mPackedValues;
        private IndexSet mIndexSet;

        public IndexedSet(int capacity)
        {
            mPackedValues = new TValue[capacity];
            mIndexSet = new IndexSet(capacity);
        }

        public void Add(TValue value, uint atIndex)
        {
            mPackedValues[mIndexSet.Count] = value;
            mIndexSet.Add(atIndex);
        }

        public bool Contains(uint index)
        {
            return mIndexSet.Contains(index);
        }

        public void Replace(TValue newValue, uint atIndex)
        {
            uint packedIndex = mIndexSet.GetPackedIndex(atIndex);
            mPackedValues[packedIndex] = newValue;
        }

        public void AddOrReplace(TValue value, uint atIndex)
        {
            if (Contains(atIndex))
            {
                Replace(value, atIndex);
            }
            else
            {
                Add(value, atIndex);
            }
        }

        public TValue GetValue(uint atIndex)
        {
            uint packedIndex = mIndexSet.GetPackedIndex(atIndex);
            return mPackedValues[packedIndex];
        }

        public void Remove(uint atIndex)
        {
            uint packedIndex = mIndexSet.GetPackedIndex(atIndex);
            mIndexSet.Remove(atIndex);
            mPackedValues[packedIndex] = mPackedValues[mIndexSet.Count];
        }
    }
}
