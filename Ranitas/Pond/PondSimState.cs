using Microsoft.Xna.Framework;
using Ranitas.Data;
using System.Collections.Generic;

namespace Ranitas.Pond
{
    public sealed class PondSimState
    {
        public readonly static Vector2 kGravity = new Vector2(0f, -1650f);    //TODO: Data drive!

        public float Width;
        public float Height;
        public float WaterLevel;

        public float[] WaterPositions;
        public float[] WaterVelocities;

        public readonly List<LilyPadSimState> Lilies;

        public PondSimState(PondData data)
        {
            Width = data.Width;
            Height = data.Height;
            WaterLevel = data.WaterLevel;
            if (data.Lilies != null)
            {
                Lilies = new List<LilyPadSimState>(data.Lilies.Length);
                foreach (var lilyData in data.Lilies)
                {
                    LilyPadSimState lilyState = new LilyPadSimState(lilyData.LilyPad, new Vector2(lilyData.HorizontalPosition, data.WaterLevel));
                    Lilies.Add(lilyState);
                }
            }
        }

        private float FindSpawnY(float forX)
        {
            float topY = WaterLevel;
            foreach (var lily in Lilies)
            {
                if ((lily.MinX() <= forX) && (forX <= lily.MaxX()))
                {
                    float topYCandidate = (lily.Height * 0.5f) + lily.Position.Y;
                    if (topY < topYCandidate)
                    {
                        topY = topYCandidate;
                    }
                }
            }
            return topY;
        }
    }
}
