using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Ranitas.Core;

namespace Ranitas.Input
{
    public sealed class InputProcessor
    {
        public const float kMinMagnitude = 0.15f;
        public const float kMaxMagnitude = 0.85f;
        public const float kRangeNormalizer = 1f / (kMaxMagnitude - kMinMagnitude);

        public readonly FrogInput[] Inputs;

        public InputProcessor(int playerCount)
        {
            Inputs = new FrogInput[playerCount];
        }

        public void ProcessInput(int playerIndex)
        {
            GamePadState state = GamePad.GetState(playerIndex);

            float scaledMagnitude = 0f;
            Vector2 direction = state.ThumbSticks.Left;
            float rawMagnitude = direction.Length();
            if (rawMagnitude >= kMinMagnitude)
            {
                scaledMagnitude = MathExtensions.Clamp01((rawMagnitude - kMinMagnitude) * kRangeNormalizer);
                direction.Normalize();
            }
            else
            {
                direction = Vector2.Zero;
            }

            FrogSignals signal = FrogSignals.None;
            if (state.IsButtonDown(Buttons.A))
            {
                signal = signal | FrogSignals.Jump;
            }
            if (state.IsButtonDown(Buttons.X))
            {
                signal = signal | FrogSignals.Toungue;
            }

            Inputs[playerIndex] = new FrogInput(direction, scaledMagnitude, signal);
        }

        public FrogInput GetFrogInput(int playerIndex)
        {
            return Inputs[playerIndex];
        }
    }
}
