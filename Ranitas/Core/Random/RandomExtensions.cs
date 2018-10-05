using System;

namespace Ranitas.Core
{
    public static class RandomExtensions
    {
        public static float GetRandomInRange(this Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        public static float NextPoissonTime(this Random random, float frequency)
        {
            double dieRoll = 1f - random.NextDouble();
            return -(float)Math.Log(dieRoll) / frequency;
        }
    }
}
