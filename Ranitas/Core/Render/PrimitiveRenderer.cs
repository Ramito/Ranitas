using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ranitas.Core.Render
{
    public sealed class PrimitiveRenderer
    {
        private VertexBuffer mVertexBuffer;
        private VertexPositionColor[] mVertexBufferData;
        private int mCurrentIndex = -1;

        public void Setup(GraphicsDevice device)
        {
            //TODO: Where should the basic effect live??
            SetupVertexBuffer(device);
        }

        public void Render(GraphicsDevice device)
        {
            if (mCurrentIndex > 0)
            {
                mVertexBuffer.SetData(mVertexBufferData, 0, mCurrentIndex);
                int triangleCount = mCurrentIndex - 2;
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, triangleCount);
                mCurrentIndex = 0;
            }
        }

        public void PushRect(Rect rect, Color color)
        {
            mVertexBufferData[mCurrentIndex].Position = new Vector3(rect.MaxCorner, 0f);
            mVertexBufferData[mCurrentIndex + 1].Position = new Vector3(rect.MaxCorner, 0f);
            mVertexBufferData[mCurrentIndex + 2].Position = new Vector3(rect.MaxMinCorner, 0f);
            mVertexBufferData[mCurrentIndex + 3].Position = new Vector3(rect.MinMaxCorner, 0f);
            mVertexBufferData[mCurrentIndex + 4].Position = new Vector3(rect.MinCorner, 0f);
            mVertexBufferData[mCurrentIndex + 5].Position = new Vector3(rect.MinCorner, 0f);

            mVertexBufferData[mCurrentIndex].Color = color;
            mVertexBufferData[mCurrentIndex + 1].Color = color;
            mVertexBufferData[mCurrentIndex + 2].Color = color;
            mVertexBufferData[mCurrentIndex + 3].Color = color;
            mVertexBufferData[mCurrentIndex + 4].Color = color;
            mVertexBufferData[mCurrentIndex + 5].Color = color;

            mCurrentIndex += 6;
        }

        private void SetupVertexBuffer(GraphicsDevice device)
        {
            const int kVertexCount = 250 * 5;
            mVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), kVertexCount, BufferUsage.WriteOnly);
            device.SetVertexBuffer(mVertexBuffer);
            mVertexBufferData = new VertexPositionColor[kVertexCount];
            mCurrentIndex = 0;
        }
    }
}
