using Microsoft.Xna.Framework;
using Ranitas.Core.ECS;

namespace Ranitas.Sim
{
    public struct ParentEntity
    {
        public ParentEntity(Entity parent, Vector2 offset)
        {
            Parent = parent;
            Offset = offset;
        }

        public Entity Parent;
        public Vector2 Offset;
    }
}
