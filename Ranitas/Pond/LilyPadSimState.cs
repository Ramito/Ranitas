using Microsoft.Xna.Framework;
using Ranitas.Data;

namespace Ranitas.Pond
{
    public sealed class LilyPadSimState
    {
        public float Width;
        public float Height;
        public Vector2 Position;    //Center of mass

        public LilyPadSimState(LilyPadData data, Vector2 position)
        {
            Width = data.Width;
            Height = data.Height;
            Position = position;
        }
    }
}
