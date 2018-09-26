using Microsoft.Xna.Framework;

namespace Ranitas.Core
{
    public struct Rect
    {
        public readonly Vector2 MinCorner;
        public readonly Vector2 MaxCorner;

        public float Width { get { return MaxX - MinX; } }
        public float Height { get { return MaxY - MinY; } }

        public Vector2 MinMaxCorner { get { return new Vector2(MinX, MaxY); } }
        public Vector2 MaxMinCorner { get { return new Vector2(MaxX, MinY); } }

        public float MaxX { get { return MaxCorner.X; } }
        public float MaxY { get { return MaxCorner.Y; } }
        public float MinX { get { return MinCorner.X; } }
        public float MinY { get { return MinCorner.Y; } }

        public Rect(Vector2 centerOfMass, float width, float height)
        {
            Vector2 halfDimensions = 0.5f * (new Vector2(width, height));
            MinCorner = centerOfMass - halfDimensions;
            MaxCorner = centerOfMass + halfDimensions;
        }

        public Rect(Vector2 corner1, Vector2 corner2)
        {
            MinMaxHelper(corner1.X, corner2.X, out float minX, out float maxX);
            MinMaxHelper(corner1.Y, corner2.Y, out float minY, out float maxY);

            MinCorner = new Vector2(minX, minY);
            MaxCorner = new Vector2(maxX, maxY);
        }

        private static void MinMaxHelper(float value1, float value2, out float min, out float max)
        {
            if (value1 <= value2)
            {
                min = value1;
                max = value2;
            }
            else
            {
                min = value2;
                max = value1;
            }
        }

        public bool Contains(Vector2 point)
        {
            return (MinX <= point.X)
                && (MaxX >= point.X)
                && (MinY <= point.Y)
                && (MaxY >= point.Y);
        }

        public bool Intersects(Rect otherRect)
        {
            return (MinX <= otherRect.MaxX)
                && (MaxX >= otherRect.MinX)
                && (MinY <= otherRect.MaxY)
                && (MaxY >= otherRect.MinY);
        }

        public Rect Inflated(float amount)
        {
            Vector2 offset = amount * Vector2.One;
            return new Rect(MinCorner - offset, MaxCorner + offset);
        }
    }
}
