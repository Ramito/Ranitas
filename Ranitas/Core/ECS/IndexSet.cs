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

        public bool Contains(uint index)
        {
            uint sparseIndex = mSparseIndices[index];
            return (sparseIndex < Count) && (mPackedIndices[sparseIndex] == index);
        }

        public void Add(uint index)
        {
            Debug.Assert(!Contains(index));
            mSparseIndices[index] = Count;
            mPackedIndices[Count] = index;
            ++Count;
        }

        public void Remove(uint index)
        {
            Debug.Assert(Contains(index));
            --Count;
            uint deletedInPacked = mSparseIndices[index];
            uint movedIndex = mPackedIndices[Count];
            mSparseIndices[movedIndex] = deletedInPacked;
            mPackedIndices[deletedInPacked] = movedIndex;
        }

        internal uint GetPackedIndex(uint index)
        {
            Debug.Assert(Contains(index));
            return mSparseIndices[index];
        }
    }

    public interface IIndexDirectory
    {
        bool Contains(uint index);
    }
}
