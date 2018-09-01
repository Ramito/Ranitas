namespace Ranitas.Core.ECS
{
    public interface IUntypedIndexedSet : IReadonlyIndexSet
    {
        void Remove(uint atIndex);
    }
}
