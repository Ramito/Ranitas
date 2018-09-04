namespace Ranitas.Core.ECS
{
    public interface IReadonlyIndexSet
    {
        bool Contains(int indexID);
        int GetPackedIndex(int indexID);
    }
}
