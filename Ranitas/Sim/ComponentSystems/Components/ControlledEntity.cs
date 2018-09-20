using Ranitas.Core.ECS;

namespace Ranitas.Sim
{
    public struct ControlledEntity
    {
        public ControlledEntity(Entity controlledEntity)
        {
            Entity = controlledEntity;
        }

        public Entity Entity;
    }
}
