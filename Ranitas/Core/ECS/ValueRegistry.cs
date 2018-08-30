using System.Diagnostics;

namespace Ranitas.Core.ECS
{
    //TODO: This class needs work!
    public class ValueRegistry<TValue> where TValue : struct
    {
        public uint Count { private set; get; }
        private TValue[] mValues;   //TODO: Dynamic size so we don't need to allocate all the things?

        public TValue this[uint index]
        {
            get
            {
                Debug.Assert(index < Count);
                return mValues[index];
            }
        }

        //TODO: Hide following interface!?!?
        public ValueRegistry(int capacity)
        {
            Count = 0;
            mValues = new TValue[capacity];
        }

        public void AddValue(TValue value)
        {
            mValues[Count] = value;
            ++Count;
        }

        //The index is managed by the index set injecting to this registry
        public void SetValue(TValue value, uint packedIndex)
        {
            Debug.Assert(Count > packedIndex);
            mValues[packedIndex] = value;
        }

        public void RemoveValue(uint packedIndex)
        {
            Debug.Assert(Count > 0);
            --Count;
            mValues[packedIndex] = mValues[Count];
        }
    }
}
