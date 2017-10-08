using Microsoft.Xna.Framework;

namespace Ranitas.Data
{
    public sealed class PondData
    {
        public float Width;
        public float Height;

        public LilliPadLocation[] Lillies;

        public float WaterLevel;
    }

    public struct LilliPadLocation
    {
        public LillyPadData LillyPad;
        public float HorizontalPosition;
    }

    public sealed class LillyPadData
    {
        public float Width;
        public float Height;
    }
}
