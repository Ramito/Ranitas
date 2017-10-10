using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Ranitas.Frog
{
    public sealed class FrogInput
    {
        public readonly int PlayerIndex;
        public readonly FrogSimState Frog;
        public FrogActionState ActionState = FrogActionState.Idle;

        public FrogInput(int playerIndex, FrogSimState frog)
        {
            PlayerIndex = playerIndex;
            Frog = frog;
        }

        public void Update()
        {
            if (ActionState == FrogActionState.Jumping)
            {
                Frog.Velocity = new Vector2(10, 700);
                ActionState = FrogActionState.Idle;
            }
            else
            {
                //TODO: Process gamepad input one layer above!
                bool jumpButtonDown = (GamePad.GetState(PlayerIndex).Buttons.A == ButtonState.Pressed);
                JumpButtonPressed(jumpButtonDown);
            }
        }

        public void JumpButtonPressed(bool pressed)
        {
            if (pressed)
            {
                if (ActionState == FrogActionState.Idle)
                {
                    ActionState = FrogActionState.PreparingJump;
                }
            }
            else
            {
                if (ActionState == FrogActionState.PreparingJump)
                {
                    ActionState = FrogActionState.Jumping;
                }
            }
        }

        public enum FrogActionState
        {
            Idle,
            PreparingJump,
            Jumping,
        }
    }
}
