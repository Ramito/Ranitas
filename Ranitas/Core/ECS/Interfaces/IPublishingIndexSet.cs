namespace Ranitas.Core.ECS
{
    public delegate void IndexedValueHandler(int indexID);

    public interface IPublishingIndexSet : IReadonlyIndexSet
    {
        event IndexedValueHandler PreNewValue;
        event IndexedValueHandler PostNewValue;
        event IndexedValueHandler ValueModified;
        event IndexedValueHandler PreRemoved;
        event IndexedValueHandler PostRemoved;
    }
}
