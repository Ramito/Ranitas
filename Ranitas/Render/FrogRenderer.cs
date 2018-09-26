using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ranitas.Core.Render
{
    public sealed class FrogRenderer
    {
        private VertexBuffer mVertexBuffer;
        private AlphaTestEffect mAlphaTestEffect;
        private VertexPositionTexture[] mVertexBufferData;
        private int mCurrentIndex = -1;
        private Texture2D mFrogSprite;

        public void Setup(GraphicsDevice device, Texture2D frogSprite)
        {
            mFrogSprite = frogSprite;
            SetupVertexBuffer(device);
            SetupEffect(device);
        }

        public void Render(Matrix cameraMatrix, GraphicsDevice device)
        {
            if (mCurrentIndex > 0)
            {
                mAlphaTestEffect.View = cameraMatrix;
                mAlphaTestEffect.CurrentTechnique.Passes[0].Apply();

                device.SetVertexBuffer(mVertexBuffer);
                device.DepthStencilState = DepthStencilState.DepthRead;
                mVertexBuffer.SetData(mVertexBufferData, 0, mCurrentIndex);
                int triangleCount = mCurrentIndex - 2;
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, triangleCount);
                mCurrentIndex = 0;
            }
        }

        public void PushFrog(Rect frogRect, int facing, bool jumping)
        {
            const float kDepth = 0f;

            frogRect = frogRect.Inflated(16.5f);

            mVertexBufferData[mCurrentIndex].Position = new Vector3(frogRect.MaxCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 1].Position = new Vector3(frogRect.MaxCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 2].Position = new Vector3(frogRect.MaxMinCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 3].Position = new Vector3(frogRect.MinMaxCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 4].Position = new Vector3(frogRect.MinCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 5].Position = new Vector3(frogRect.MinCorner, kDepth);

            float frameOffset = 0f;
            if (jumping)
            {
                frameOffset = -0.5f;
            }
            float maxX = 1f + frameOffset;
            float minX = 0.5f + frameOffset;
            if (facing < 0)
            {
                float swap = maxX;
                maxX = minX;
                minX = swap;
            }

            float topY = 0f;
            float bottomY = 1f;

            mVertexBufferData[mCurrentIndex].TextureCoordinate = new Vector2(maxX, topY);
            mVertexBufferData[mCurrentIndex + 1].TextureCoordinate = new Vector2(maxX, topY);
            mVertexBufferData[mCurrentIndex + 2].TextureCoordinate = new Vector2(maxX, bottomY);
            mVertexBufferData[mCurrentIndex + 3].TextureCoordinate = new Vector2(minX, topY);
            mVertexBufferData[mCurrentIndex + 4].TextureCoordinate = new Vector2(minX, bottomY);
            mVertexBufferData[mCurrentIndex + 5].TextureCoordinate = new Vector2(minX, bottomY);

            mCurrentIndex += 6;
        }

        private void SetupVertexBuffer(GraphicsDevice device)
        {
            const int kVertexCount = 4 * 6;
            mVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), kVertexCount, BufferUsage.WriteOnly);
            mVertexBufferData = new VertexPositionTexture[kVertexCount];
            mCurrentIndex = 0;
        }

        private void SetupEffect(GraphicsDevice device)
        {
            mAlphaTestEffect = new AlphaTestEffect(device);
            mAlphaTestEffect.World = Matrix.Identity;
            mAlphaTestEffect.Alpha = 1;
            mAlphaTestEffect.Texture = mFrogSprite;
        }
    }
}
