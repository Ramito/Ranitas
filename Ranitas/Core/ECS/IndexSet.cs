using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    public class IndexSet : IIndexDirectory
    {
        public uint Count { get; private set; }
        private uint[] mSparseIndices;
        private uint[] mPackedIndices;

        public IndexSet(int capacity)
        {
            Count = 0;
            mSparseIndices = new uint[capacity];
            mPackedIndices = new uint[capacity];
        }

        public bool Contains(uint indexID)
        {
            uint sparseIndex = mSparseIndices[indexID];
            return (sparseIndex < Count) && (mPackedIndices[sparseIndex] == indexID);
        }

        public void Add(uint indexID)
        {
            Debug.Assert(!Contains(indexID));
            mSparseIndices[indexID] = Count;
            mPackedIndices[Count] = indexID;
            ++Count;
        }

        public void Remove(uint indexID)
        {
            Debug.Assert(Contains(indexID));
            --Count;
            uint deletedInPacked = mSparseIndices[indexID];
            uint movedIndex = mPackedIndices[Count];
            mSparseIndices[movedIndex] = deletedInPacked;
            mPackedIndices[deletedInPacked] = movedIndex;
        }

        internal uint GetPackedIndex(uint indexID)
        {
            Debug.Assert(Contains(indexID));
            return mSparseIndices[indexID];
        }
    }

    public interface IIndexDirectory
    {
        bool Contains(uint indexID);
    }
}
