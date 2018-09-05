using Ranitas.Core.ECS;
using Ranitas.Core.EventSystem;

namespace Ranitas.Sim
{
    public interface ISystem
    {
        void Initialize(EntityRegistry registry, EventSystem eventSystem);
        void Update(EntityRegistry registry, EventSystem eventSystem);
    }
}
