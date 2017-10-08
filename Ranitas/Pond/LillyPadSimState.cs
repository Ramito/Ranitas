using Microsoft.Xna.Framework;
using Ranitas.Data;

namespace Ranitas.Pond
{
    public sealed class LillyPadSimState
    {
        public float Width;
        public float Height;
        public Vector2 Position;    //Center of mass

        public LillyPadSimState(LillyPadData data, Vector2 position)
        {
            Width = data.Width;
            Height = data.Height;
            Position = position;
        }
    }
}
