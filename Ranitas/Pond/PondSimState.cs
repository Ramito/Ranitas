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

        public readonly List<LillyPadSimState> Lillies;

        public PondSimState(PondData data)
        {
            Width = data.Width;
            Height = data.Height;
            WaterLevel = data.WaterLevel;
            if (data.Lillies != null)
            {
                Lillies = new List<LillyPadSimState>(data.Lillies.Length);
                foreach (var lillyData in data.Lillies)
                {
                    LillyPadSimState lillyState = new LillyPadSimState(lillyData.LillyPad, new Vector2(lillyData.HorizontalPosition, data.WaterLevel));
                    Lillies.Add(lillyState);
                }
            }
        }
    }
}
