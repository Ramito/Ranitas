using System.Diagnostics;

public class RestrictedArray<TValue> where TValue : struct
{
    public uint Count { private set; get; }  //TODO: All those uints need to go!
    private readonly TValue[] mValues;

    public TValue this[uint index]  //TODO: All those uints need to go!
    {
        get
        {
            Debug.Assert(index < Count);
            return mValues[index];
        }
    }

    public RestrictedArray(int capacity)
    {
        Count = 0;
        mValues = new TValue[capacity];
    }

    public void AddValue(TValue value)
    {
        mValues[Count] = value;
        ++Count;
    }

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
