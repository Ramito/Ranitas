using Microsoft.Xna.Framework;
using Ranitas.Data;

namespace Ranitas.Frog
{
    public sealed class FrogSimState
    {
        public float Width;
        public float Height;
        public Vector2 Position;    //Center of mass

        public Vector2 Velocity;

        public enum FrogState
        {
            Grounded,
            Airborne,
            Swimming,
        }
        public FrogState State;

        public Vector2 FeetPosition
        {
            get { return Position - new Vector2(0f, Height * 0.5f); }
            set { Position = value + new Vector2(0f, Height * 0.5f); }
        }

        public FrogSimState(FrogData data)
        {
            Width = data.Width;
            Height = data.Height;
            State = FrogState.Grounded;
            Velocity = Vector2.Zero;
        }
    }
}
