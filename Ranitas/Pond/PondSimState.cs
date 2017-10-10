using Microsoft.Xna.Framework;
using Ranitas.Data;
using Ranitas.Frog;
using System.Collections.Generic;

namespace Ranitas.Pond
{
    public sealed class PondSimState
    {
        public float Width;
        public float Height;
        public float WaterLevel;

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

        public FrogSimState SpawnFrog(FrogData frogData, float[] frogSpawns, int spawnIndex)
        {
            spawnIndex %= frogSpawns.Length;
            FrogSimState frogSimState = new FrogSimState(frogData);
            float spawnX = frogSpawns[spawnIndex];
            float spawnY = FindSpawnY(spawnX) + 500;
            frogSimState.FeetPosition = new Vector2(spawnX, spawnY);
            return frogSimState;
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
