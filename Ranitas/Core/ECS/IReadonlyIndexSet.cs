namespace Ranitas.Core.ECS
{
    public interface IReadonlyIndexSet
    {
        bool Contains(uint indexID);
    }

    public interface IPublishingIndexSet : IReadonlyIndexSet
    {
        event IndexedValueHandler NewValue;
        event IndexedValueHandler ValueModified;
        event IndexedValueHandler Removed;
    }
}
