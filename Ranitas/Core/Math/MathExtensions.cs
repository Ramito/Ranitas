using Microsoft.Xna.Framework;

namespace Ranitas.Core
{
    public static class MathExtensions
    {
        public static float Clamp01(float value)
        {
            return MathHelper.Clamp(value, 0f, 1f);
        }
    }
}
