using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ranitas.Data;
using Ranitas.Sim;

namespace Ranitas.Core.Render
{
    public sealed class FrogRenderer
    {
        private float mSpriteOffset;
        private Texture2D mFrogSprite;
        private VertexBuffer mVertexBuffer;
        public AlphaTestEffect mAlphaTestEffect;
        private VertexPositionTexture[] mVertexBufferData;
        private int mCurrentIndex = -1;

        public void Setup(GraphicsDevice device, Texture2D frogSprite, FrogAnimationData data)
        {
            mSpriteOffset = data.SpriteCornerOffset;
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
            device.BlendState = BlendState.AlphaBlend;
            device.RasterizerState = RasterizerState.CullClockwise;
            device.SamplerStates[0] = SamplerState.PointClamp;
        }

        public void PushFrog(Rect frogRect, AnimationState animationState)
        {
            const float kDepth = 1f;

            frogRect = frogRect.Inflated(mSpriteOffset);

            mVertexBufferData[mCurrentIndex].Position = new Vector3(frogRect.MaxCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 1].Position = new Vector3(frogRect.MaxCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 2].Position = new Vector3(frogRect.MaxMinCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 3].Position = new Vector3(frogRect.MinMaxCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 4].Position = new Vector3(frogRect.MinCorner, kDepth);
            mVertexBufferData[mCurrentIndex + 5].Position = new Vector3(frogRect.MinCorner, kDepth);

            float topY = 0f;
            float bottomY = 1f;

            mVertexBufferData[mCurrentIndex].TextureCoordinate = new Vector2(animationState.MaxX, topY);
            mVertexBufferData[mCurrentIndex + 1].TextureCoordinate = new Vector2(animationState.MaxX, topY);
            mVertexBufferData[mCurrentIndex + 2].TextureCoordinate = new Vector2(animationState.MaxX, bottomY);
            mVertexBufferData[mCurrentIndex + 3].TextureCoordinate = new Vector2(animationState.MinX, topY);
            mVertexBufferData[mCurrentIndex + 4].TextureCoordinate = new Vector2(animationState.MinX, bottomY);
            mVertexBufferData[mCurrentIndex + 5].TextureCoordinate = new Vector2(animationState.MinX, bottomY);

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
