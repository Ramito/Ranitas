namespace Ranitas.Core.ECS
{
    public delegate void IndexedValueHandler(int indexID);

    public interface IPublishingIndexSet : IReadonlyIndexSet
    {
        event IndexedValueHandler NewValue;
        event IndexedValueHandler ValueModified;
        event IndexedValueHandler Removed;
    }
}
