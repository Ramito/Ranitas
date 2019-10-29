﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Ranitas.Core.Render
{
    public sealed class PrimitiveRenderer
    {
        private VertexBuffer mVertexBuffer;
        private VertexPositionColor[] mVertexBufferData;
        private int mCurrentIndex = 0;
        private int mCurrentShapeBegin = -1;

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

        public void StartShape()
        {
            ValidateShapeDrawingScope(false);
            ToggleShapeDrawingScope();
        }

        public void ShapeVertex(Vector2 vertex, Color vertexColor)
        {
            ValidateShapeDrawingScope(true);
            const float kDepth = 0f;
            if (mCurrentShapeBegin == mCurrentIndex)
            {
                mVertexBufferData[mCurrentIndex] = new VertexPositionColor(new Vector3(vertex, kDepth), vertexColor);
                ++mCurrentIndex;
            }
            mVertexBufferData[mCurrentIndex] = new VertexPositionColor(new Vector3(vertex, kDepth), vertexColor);
            ++mCurrentIndex;
        }

        public void EndShape()
        {
            ValidateShapeDrawingScope(true);
            mVertexBufferData[mCurrentIndex] = mVertexBufferData[mCurrentIndex - 1];
            ++mCurrentIndex;
            ToggleShapeDrawingScope();
        }

        public void PushRect(Rect rect, Color color)
        {
            StartShape();
            ShapeVertex(rect.MaxCorner, color);
            ShapeVertex(rect.MaxMinCorner, color);
            ShapeVertex(rect.MinMaxCorner, color);
            ShapeVertex(rect.MinCorner, color);
            EndShape();
        }

        private void SetupVertexBuffer(GraphicsDevice device)
        {
            const int kVertexCount = 250 * 6;
            mVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), kVertexCount, BufferUsage.WriteOnly);
            mVertexBufferData = new VertexPositionColor[kVertexCount];
            mCurrentIndex = 0;
        }

        private void ToggleShapeDrawingScope()
        {
            if (mCurrentShapeBegin == -1)
            {
                mCurrentShapeBegin = mCurrentIndex;
            }
            else
            {
                mCurrentShapeBegin = -1;
            }
        }

        [Conditional("DEBUG")]
        private void ValidateShapeDrawingScope(bool expectedScope)
        {
            Debug.Assert((mCurrentShapeBegin == -1) != expectedScope);
        }
    }
}
