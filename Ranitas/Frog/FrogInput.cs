using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
            if (controlModule < 0.25f)
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
                    }
                }
                else
                {
                    BoundFrog.PreparingJump = false;
                    BoundFrog.JumpSignaled = true;
                    BoundFrog.SignaledJumpDirection = ControlDirection;
                }
                JumpButtonState = jumpButtonState;
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
