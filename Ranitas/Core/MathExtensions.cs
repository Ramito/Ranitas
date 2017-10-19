namespace Ranitas.Core
{
    public static class MathExtensions
    {
        public static float Clamp01(float value)
        {
            if (value >= 1f)
            {
                return 1f;
            }
            if (value <= 0f)
            {
                return 0f;
            }
            return value;
        }
    }
}
