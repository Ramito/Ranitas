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
            SetupVertexBuffer(device);
        }

        public void RenderAndFlush(GraphicsDevice device, Effect effect)
        {
            if (mCurrentIndex > 0)
            {
                effect.CurrentTechnique.Passes[0].Apply();
                SetDeviceStates(device);
                mVertexBuffer.SetData(mVertexBufferData, 0, mCurrentIndex);
                int triangleCount = mCurrentIndex - 2;
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, triangleCount);
                mCurrentIndex = 0;
            }
        }

        private void SetDeviceStates(GraphicsDevice device)
        {
            device.SetVertexBuffer(mVertexBuffer);
            device.DepthStencilState = DepthStencilState.DepthRead;
            device.BlendState = BlendState.NonPremultiplied;
            device.RasterizerState = RasterizerState.CullClockwise;
            device.SamplerStates[0] = SamplerState.PointClamp;
        }

        public void PushRect(Rect rect, Color color)
        {
            const float kDepth = 0f;

            mVertexBufferData[mCurrentIndex] = new VertexPositionColor(new Vector3(rect.MaxCorner, kDepth), color);
            mVertexBufferData[mCurrentIndex + 1] = new VertexPositionColor(new Vector3(rect.MaxCorner, kDepth), color);
            mVertexBufferData[mCurrentIndex + 2] = new VertexPositionColor(new Vector3(rect.MaxMinCorner, kDepth), color);
            mVertexBufferData[mCurrentIndex + 3] = new VertexPositionColor(new Vector3(rect.MinMaxCorner, kDepth), color);
            mVertexBufferData[mCurrentIndex + 4] = new VertexPositionColor(new Vector3(rect.MinCorner, kDepth), color);
            mVertexBufferData[mCurrentIndex + 5] = new VertexPositionColor(new Vector3(rect.MinCorner, kDepth), color);

            mCurrentIndex += 6;
        }

        private void SetupVertexBuffer(GraphicsDevice device)
        {
            const int kVertexCount = 250 * 6;
            mVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), kVertexCount, BufferUsage.WriteOnly);
            mVertexBufferData = new VertexPositionColor[kVertexCount];
            mCurrentIndex = 0;
        }
    }
}
