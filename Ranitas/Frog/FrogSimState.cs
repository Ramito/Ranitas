using Microsoft.Xna.Framework;
using Ranitas.Data;
using System;

namespace Ranitas.Frog
{
    public sealed class FrogSimState
    {
        public readonly FrogData Prototype;

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

        public bool PreparingJump;
        public bool JumpSignaled;
        public Vector2 SignaledJumpDirection;

        public float SwimKickPhase;
        public Vector2 SwimDirection;

        public float TimePreparingJump;
        public float PreparedJumpPercentage
        {
            get
            {
                return Math.Min(1f, TimePreparingJump / Prototype.JumpPrepareTime);
            }
        }

        public Vector2 FeetPosition
        {
            get { return Position - new Vector2(0f, Height * 0.5f); }
            set { Position = value + new Vector2(0f, Height * 0.5f); }
        }

        public FrogSimState(FrogData data)
        {
            Prototype = data;
            Width = data.Width;
            Height = data.Height;
            State = FrogState.Grounded;
            Velocity = Vector2.Zero;
            PreparingJump = false;
            TimePreparingJump = 0f;
        }
    }
}
