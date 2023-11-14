namespace Ranitas.Core.ECS
{
    public interface IIndexedSet<TValue> : IReadonlyIndexedSet<TValue>, IUntypedIndexedSet where TValue : struct
    {
        void Add(TValue value, int indexID);
        void Replace(TValue newValue, int indexID);
    }
}