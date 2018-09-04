using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    public class IndexSet : IReadonlyIndexSet
    {
        public int Count { get; private set; }
        private int[] mSparseIndices;
        private int[] mPackedIndices;

        public IndexSet(int capacity)
        {
            Count = 0;
            mSparseIndices = new int[capacity];
            mPackedIndices = new int[capacity];
        }

        public bool Contains(int indexID)
        {
            int sparseIndex = mSparseIndices[indexID];
            return (sparseIndex < Count) && (mPackedIndices[sparseIndex] == indexID);
        }

        public void Add(int indexID)
        {
            Debug.Assert(!Contains(indexID));
            mSparseIndices[indexID] = Count;
            mPackedIndices[Count] = indexID;
            ++Count;
        }

        public void Remove(int indexID)
        {
            Debug.Assert(Contains(indexID));
            --Count;
            int deletedInPacked = mSparseIndices[indexID];
            int movedIndex = mPackedIndices[Count];
            mSparseIndices[movedIndex] = deletedInPacked;
            mPackedIndices[deletedInPacked] = movedIndex;
        }

        public int GetPackedIndex(int indexID)
        {
            Debug.Assert(Contains(indexID));
            return mSparseIndices[indexID];
        }
    }
}
