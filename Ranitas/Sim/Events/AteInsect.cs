using Ranitas.Core.ECS;

namespace Ranitas.Sim.Events
{
    public struct AteInsect
    {
        public AteInsect(Entity eatenBy)
        {
            EatenBy = eatenBy;
        }

        public Entity EatenBy;
    }
}
