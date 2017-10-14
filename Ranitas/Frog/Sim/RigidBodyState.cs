using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Data;

namespace Ranitas.Frog.Sim
{
    public sealed class RigidBodyState
    {
        public float Width;
        public float Height;
        public Vector2 Position;    //Center of mass
        public Vector2 Velocity;

        public float Density;

        public Rect Rect { get { return new Rect(Position, Width, Height); } }

        public Vector2 FeetPosition
        {
            get { return Position - new Vector2(0f, Height * 0.5f); }
            set { Position = value + new Vector2(0f, Height * 0.5f); }
        }

        public RigidBodyState(FrogData frogData)
        {
            Width = frogData.Width;
            Height = frogData.Height;
            Velocity = Vector2.Zero;
            Density = frogData.FrogDensity;
        }
    }
}
