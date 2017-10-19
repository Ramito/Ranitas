using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Data;
using System;

namespace Ranitas.Frog.Sim
{
    public sealed class FrogSimState
    {
        public readonly FrogData Prototype;

        public readonly RigidBodyState RigidBodyState;

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
        public float RelativeJumpStrength { get { return Math.Min(1f, TimePreparingJump / Prototype.JumpPrepareTime); } }

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

        public readonly FrogToungue Toungue;
        public Rect GetToungueRect()
        {
            Rect frogRect = RigidBodyState.Rect;
            Vector2 extendDirection;
            Vector2 anchor;
            Vector2 facing = SignaledJumpDirection; //TODO GENERAL TOUNGUE DIRECTION!
            if (facing.X >= 0f)
            {
                extendDirection = Vector2.UnitX;
                anchor = frogRect.MaxCorner;
            }
            else
            {
                extendDirection = -Vector2.UnitX;
                anchor = frogRect.MinMaxCorner;
            }
            anchor -= (Prototype.ToungueThickness * Vector2.UnitY);
            Vector2 otherCorner = anchor + Prototype.ToungueLength * Toungue.RelativeLength * extendDirection - (Prototype.ToungueThickness * Vector2.UnitY);
            return new Rect(anchor, otherCorner);
        }

        public FrogSimState(FrogData data)
        {
            Prototype = data;
            RigidBodyState = new RigidBodyState(data);
            State = FrogState.Grounded;
            PreparingJump = false;
            TimePreparingJump = 0f;
            Toungue = new FrogToungue(data);
        }
    }
}
