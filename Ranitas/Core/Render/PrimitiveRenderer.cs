using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ranitas.Core.Render
{
    public sealed class PrimitiveRenderer
    {
        private BasicEffect mEffect;
        private VertexBuffer mVertexBuffer;
        private VertexPositionColor[] mVertexBufferData;
        private int mCurrentIndex = -1;

        public void Setup(GraphicsDevice device)
        {
            SetupVertexBuffer(device);
            SetupEffect(device);
        }

        public void Render(Matrix camera, GraphicsDevice device)
        {
            if (mCurrentIndex > 0)
            {
                mEffect.View = camera;
                mEffect.CurrentTechnique.Passes[0].Apply();
                SetDeviceStates(device);
                mVertexBuffer.SetData(mVertexBufferData, 0, mCurrentIndex);
                int triangleCount = mCurrentIndex - 2;
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, triangleCount);
                mCurrentIndex = 0;
            }
        }

        public void Render(GraphicsDevice device, Effect effect)
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

        private void SetupVertexBuffer(GraphicsDevice device)
        {
            const int kVertexCount = 250 * 6;
            mVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), kVertexCount, BufferUsage.WriteOnly);
            mVertexBufferData = new VertexPositionColor[kVertexCount];
            mCurrentIndex = 0;
        }

        private void SetupEffect(GraphicsDevice device)
        {
            mEffect = new BasicEffect(device);
            mEffect.VertexColorEnabled = true;
            mEffect.World = Matrix.Identity;
            mEffect.Alpha = 1;
            mEffect.TextureEnabled = false;
        }
    }
}
