namespace Ranitas.Core.ECS
{
    public interface IIndexedSet<TValue> : IReadonlyIndexSet, IUntypedIndexedSet where TValue : struct
    {
        void Add(TValue value, uint indexID);
        void AddOrReplace(TValue value, uint indexID);
        uint GetPackedIndex(uint indexID);
        TValue GetValue(uint indexID);
        void Replace(TValue newValue, uint indexID);
    }
}