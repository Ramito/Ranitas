using Microsoft.Xna.Framework;
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

        public FrogSimState(FrogData data)
        {
            Prototype = data;
            RigidBodyState = new RigidBodyState(data);
            State = FrogState.Grounded;
            PreparingJump = false;
            TimePreparingJump = 0f;
        }
    }
}
