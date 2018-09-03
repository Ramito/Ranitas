namespace Ranitas.Core.ECS
{
    public interface IReadonlyIndexSet
    {
        bool Contains(uint indexID);
        uint GetPackedIndex(uint indexID);
    }
}
