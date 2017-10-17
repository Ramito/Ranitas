using Microsoft.Xna.Framework;
using Ranitas.Frog.Sim;

namespace Ranitas.Frog
{
    public sealed class FrogInput
    {
        public readonly FrogSimState BoundFrog;
        public Vector2 ControlDirection { get; private set; }
        public bool JumpButtonState { get; private set; }

        public FrogInput(FrogSimState boundFrog)
        {
            BoundFrog = boundFrog;
        }

        public void Update(Vector2 controlDirection, bool jumpButtonState)
        {
            float controlModule = controlDirection.LengthSquared();
            if (controlModule < 0.1f)
            {
                controlDirection = Vector2.Zero;
            }
            ControlDirection = controlDirection;
            if (jumpButtonState != JumpButtonState)
            {
                if (jumpButtonState)
                {
                    if ((BoundFrog.State == FrogSimState.FrogState.Grounded) && !BoundFrog.JumpSignaled)
                    {
                        BoundFrog.PreparingJump = true;
                        BoundFrog.TimePreparingJump = 0f;
                        JumpButtonState = jumpButtonState;
                    }
                }
                else
                {
                    BoundFrog.PreparingJump = false;
                    BoundFrog.JumpSignaled = true;
                    BoundFrog.SignaledJumpDirection = ControlDirection;
                    JumpButtonState = jumpButtonState;
                }
            }
            if (BoundFrog.State == FrogSimState.FrogState.Swimming)
            {
                if ((BoundFrog.SwimKickPhase >= BoundFrog.Prototype.SwimKickDuration) || (ControlDirection == Vector2.Zero))
                {
                    BoundFrog.SwimDirection = ControlDirection;
                }
            }
        }
    }
}
