using System.Diagnostics;

public class RestrictedArray<TValue> where TValue : struct
{
    public int Count { private set; get; }
    private readonly TValue[] mValues;

    public TValue this[int index]
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

    public void SetValue(TValue value, int packedIndex)
    {
        Debug.Assert(Count > packedIndex);
        mValues[packedIndex] = value;
    }

    public void RemoveValue(int packedIndex)
    {
        Debug.Assert(Count > 0);
        --Count;
        mValues[packedIndex] = mValues[Count];
    }
}
