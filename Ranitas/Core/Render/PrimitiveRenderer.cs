using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ranitas.Core.Render
{
    public sealed class PrimitiveRenderer
    {
        public GraphicsDevice Device { get; private set; }
        private VertexBuffer mVertexBuffer;
        private VertexPositionColor[] mVertexBufferData;
        private int mCurrentIndex = -1;

        public void Setup(GraphicsDevice device)
        {
            //TODO: Where should the basic effect live??
            Device = device;
            SetupVertexBuffer();
        }

        public void Render()
        {
            if (mCurrentIndex > 0)
            {
                Device.SetVertexBuffer(mVertexBuffer);
                Device.DepthStencilState = DepthStencilState.DepthRead;
                mVertexBuffer.SetData(mVertexBufferData, 0, mCurrentIndex);
                int triangleCount = mCurrentIndex - 2;
                Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, triangleCount);
                mCurrentIndex = 0;
            }
        }

        public void PushRect(Rect rect, Color color)
        {
            const float kDepth = 0f;

            mVertexBufferData[mCurrentIndex].Position = new Vector3(rect.MaxCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 1].Position = new Vector3(rect.MaxCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 2].Position = new Vector3(rect.MaxMinCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 3].Position = new Vector3(rect.MinMaxCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 4].Position = new Vector3(rect.MinCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 5].Position = new Vector3(rect.MinCorner, kDepth);

            mVertexBufferData[mCurrentIndex].Color = color;
            mVertexBufferData[mCurrentIndex + 1].Color = color;
            mVertexBufferData[mCurrentIndex + 2].Color = color;
            mVertexBufferData[mCurrentIndex + 3].Color = color;
            mVertexBufferData[mCurrentIndex + 4].Color = color;
            mVertexBufferData[mCurrentIndex + 5].Color = color;

            mCurrentIndex += 6;
        }

        private void SetupVertexBuffer()
        {
            const int kVertexCount = 250 * 6;
            mVertexBuffer = new VertexBuffer(Device, typeof(VertexPositionColor), kVertexCount, BufferUsage.WriteOnly);
            mVertexBufferData = new VertexPositionColor[kVertexCount];
            mCurrentIndex = 0;
        }
    }
}
