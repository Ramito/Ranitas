namespace Ranitas.Core.ECS
{
    public interface IUntypedIndexedSet : IReadonlyIndexSet
    {
        void Remove(int atIndex);
    }
}
