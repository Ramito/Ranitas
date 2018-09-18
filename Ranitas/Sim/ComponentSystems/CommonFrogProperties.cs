﻿using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Pond;

namespace Ranitas.Sim
{
    public static class CommonFrogProperties
    {
        public static Rect FrogRect(Position position, RectShape shape)
        {
            return new Rect(position.Value, shape.Width, shape.Height);
        }

        public static Vector2 FrogFeetPosition(Position position, RectShape shape)
        {
            return position.Value - new Vector2(0f, shape.Height * 0.5f);
        }

        public static Vector2 ComputeBuouyancyAcceleration(Rect frogRect, float frogDensity, PondSimState pondState)
        {
            float volumePercentage = (pondState.WaterLevel - frogRect.MinY) / (frogRect.Height);
            volumePercentage = MathExtensions.Clamp01(volumePercentage);
            return (frogDensity - volumePercentage) * PondSimState.kGravity;
        }
    }
}