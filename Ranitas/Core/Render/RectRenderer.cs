using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ranitas.Core.Render
{
    public sealed class RectRenderer
    {
        private VertexBuffer mVertexBuffer;
        private VertexPositionColor[] mVertexBufferData;

        public void Setup(GraphicsDevice device)
        {
            //TODO: Where should the basic effect live??
            SetupVertexBuffer(device);
        }

        public void RenderRect(Rect rect, Color color, GraphicsDevice device)
        {
            SetVertices(rect, color);
            mVertexBuffer.SetData(mVertexBufferData);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        private void SetVertices(Rect rect, Color color)
        {
            mVertexBufferData[0].Position = new Vector3(rect.MaxCorner, 0f);
            mVertexBufferData[1].Position = new Vector3(rect.MaxMinCorner, 0f);
            mVertexBufferData[2].Position = new Vector3(rect.MinMaxCorner, 0f);
            mVertexBufferData[3].Position = new Vector3(rect.MinCorner, 0f);

            mVertexBufferData[0].Color = color;
            mVertexBufferData[1].Color = color;
            mVertexBufferData[2].Color = color;
            mVertexBufferData[3].Color = color;
        }

        private void SetupVertexBuffer(GraphicsDevice device)
        {
            mVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), 4, BufferUsage.WriteOnly);
            device.SetVertexBuffer(mVertexBuffer);
            mVertexBufferData = CreateRectVertices();
        }

        private static VertexPositionColor[] CreateRectVertices()
        {
            VertexPositionColor[] vertices = new VertexPositionColor[4];
            return vertices;
        }
    }
}
