using Microsoft.Xna.Framework;
using System;

namespace Ranitas.Input
{
    [Flags]
    public enum FrogSignals
    {
        None = 0,
        Jump = 1 << 0,
        Toungue = 1 << 1,
    }

    public struct FrogInput
    {
        public readonly Vector2 NormalizedDirection;
        public readonly float Magnitude;
        public readonly FrogSignals Signals;

        public FrogInput(Vector2 normalizedDirection, float magnitude, FrogSignals signals)
        {
            NormalizedDirection = normalizedDirection;
            Magnitude = magnitude;
            Signals = signals;
        }
    }
}
