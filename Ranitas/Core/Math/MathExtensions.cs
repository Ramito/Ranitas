using Microsoft.Xna.Framework;
using System;

namespace Ranitas.Core
{
    public static class MathExtensions
    {
        public static float Clamp01(float value)
        {
            return MathHelper.Clamp(value, 0f, 1f);
        }

        public static Vector2 Rotate(Vector2 vectorToRotate, float angleToRotate)
        {
            float cos = (float)Math.Cos(angleToRotate);
            float sin = (float)Math.Sin(angleToRotate);
            float newX = cos * vectorToRotate.X - sin * vectorToRotate.Y;
            float newY = sin * vectorToRotate.X + cos * vectorToRotate.Y;
            return new Vector2(newX, newY);
        }
    }
}
