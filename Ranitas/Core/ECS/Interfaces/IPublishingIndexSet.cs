namespace Ranitas.Core.ECS
{
    public delegate void SetIndexHandler(int indexID);

    public interface IPublishingIndexSet : IReadonlyIndexSet
    {
        event SetIndexHandler PreNewValue;
        event SetIndexHandler PostNewValue;
        event SetIndexHandler PreRemoved;
        event SetIndexHandler PostRemoved;
    }
}
