using Ranitas.Core;

namespace Ranitas.Core.ECS
{
    public interface ISystem
    {
        void Initialize(EntityRegistry registry, EventSystem eventSystem);
        void Update(EntityRegistry registry, EventSystem eventSystem);
    }
}
