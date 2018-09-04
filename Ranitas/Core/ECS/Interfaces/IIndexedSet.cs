namespace Ranitas.Core.ECS
{
    public interface IIndexedSet<TValue> : IReadonlyIndexedSet<TValue>, IReadonlyIndexSet, IUntypedIndexedSet where TValue : struct
    {
        void Add(TValue value, int indexID);
        void AddOrReplace(TValue value, int indexID);
        void Replace(TValue newValue, int indexID);
    }
}