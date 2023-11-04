using Microsoft.Xna.Framework;
using Ranitas.Core.ECS;

namespace Ranitas.Sim.Events
{
    public struct AteInsect
    {
        public Entity EatenBy;
        public Vector2 InsectPosition;
    }
}
