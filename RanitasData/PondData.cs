using Microsoft.Xna.Framework;

namespace Ranitas.Data
{
    public sealed class PondData
    {
        public float Width;
        public float Height;

        public LiliPadLocation[] Lilies;
        public float[] FrogSpawns;

        public float WaterLevel;
    }

    public struct LiliPadLocation
    {
        public LilyPadData LilyPad;
        public float HorizontalPosition;
    }

    public sealed class LilyPadData
    {
        public float Width;
        public float Height;
    }
}
