namespace Ranitas.Core.ECS
{
    public interface IReadonlyIndexedSet<TValue> where TValue : struct
    {
        TValue GetValue(int indexID);
    }
}
