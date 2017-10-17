using Microsoft.Xna.Framework;

namespace Ranitas.Insects
{
    public class FlySimState
    {
        public float Width;
        public float Height;
        public float Speed;
        public Vector2 Position;

        public FlySimState() { }

        public void Set(float speed, Vector2 position)
        {
            Speed = speed;
            Position = position;
        }
    }
}
