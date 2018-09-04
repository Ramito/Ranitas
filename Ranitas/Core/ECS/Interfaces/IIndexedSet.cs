namespace Ranitas.Core.ECS
{
    public interface IIndexedSet<TValue> : IReadonlyIndexedSet<TValue>, IReadonlyIndexSet, IUntypedIndexedSet where TValue : struct
    {
        void Add(TValue value, uint indexID);
        void AddOrReplace(TValue value, uint indexID);
        void Replace(TValue newValue, uint indexID);
    }
}