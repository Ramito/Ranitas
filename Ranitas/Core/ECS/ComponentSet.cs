namespace Ranitas.Core.ECS
{
    public class ComponentSet<TValue> : IUntypedComponentSet, IIndexedSet<TValue> where TValue : struct
    {
        public ComponentSet(int capacity)
        {
            mIndexedSet = new IndexedSet<TValue>(capacity);
        }

        private readonly IndexedSet<TValue> mIndexedSet;

        public event SetIndexHandler PreNewValue;
        public event SetIndexHandler PostNewValue;
        public event SetIndexHandler PreRemoved;
        public event SetIndexHandler PostRemoved;


        public delegate void IndexedValueHandler(int indexID, TValue value);
        public event IndexedValueHandler ValueModified;

        public void Add(TValue value, int indexID)
        {
            PreNewValue?.Invoke(indexID);
            mIndexedSet.Add(value, indexID);
            PostNewValue?.Invoke(indexID);
        }

        public void AddOrReplace(TValue value, int indexID)
        {
            mIndexedSet.AddOrReplace(value, indexID);
        }

        public bool Contains(int indexID)
        {
            return mIndexedSet.Contains(indexID);
        }

        public int GetPackedIndex(int indexID)
        {
            return mIndexedSet.GetPackedIndex(indexID);
        }

        public TValue GetValue(int indexID)
        {
            return mIndexedSet.GetValue(indexID);
        }

        public void Remove(int atIndex)
        {
            PreRemoved?.Invoke(atIndex);
            mIndexedSet.Remove(atIndex);
            PostRemoved?.Invoke(atIndex);
        }

        public void Replace(TValue newValue, int indexID)
        {
            mIndexedSet.Replace(newValue, indexID);
            ValueModified?.Invoke(indexID, newValue);
        }
    }
}
