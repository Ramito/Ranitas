using Microsoft.Xna.Framework;
using Ranitas.Data;
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
    }
}
