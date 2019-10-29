using Microsoft.Xna.Framework;
using Ranitas.Core.ECS;

namespace Ranitas.Sim
{
    public sealed class FlyFactory
    {
        public FlyFactory(EntityRegistry registry)
        {
            mRegistry = registry;
        }

        private EntityRegistry mRegistry;

        public Entity MakeFly(Vector2 position, Vector2 velocity, float width, float height)
        {
            Entity fly = mRegistry.Create();

            mRegistry.AddComponent(fly, new Insect());
            mRegistry.AddComponent(fly, new Position(position));
            mRegistry.AddComponent(fly, new Velocity(velocity));
            mRegistry.AddComponent(fly, new RectShape(width, height));
            mRegistry.AddComponent(fly, Color.Black);

            return fly;
        }
    }
}
