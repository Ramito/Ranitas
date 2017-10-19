using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ranitas.Core;
using Ranitas.Frog.Sim;

namespace Ranitas.Frog
{
    public sealed class FrogRenderer
    {
        private VertexBuffer mVertexBuffer;
        private VertexPositionColor[] mVertexBufferData;

        public void Setup(GraphicsDevice device)
        {
            //TODO: Where should the basic effect live??
            SetupVertexBuffer(device);
        }

        public void RenderFrog(FrogSimState frog, GraphicsDevice device)
        {
            SetVertices(mVertexBufferData, frog.RigidBodyState.Rect);
            mVertexBuffer.SetData(mVertexBufferData);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            float relativeToungueLength = frog.Toungue.RelativeLength;
            if (relativeToungueLength > 0f)
            {
                SetVertices(mVertexBufferData, frog.GetToungueRect());
                mVertexBuffer.SetData(mVertexBufferData);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

        private static void SetVertices(VertexPositionColor[] vertices, Rect frogRect)
        {
            vertices[0].Position = new Vector3(frogRect.MaxCorner, 0f);
            vertices[1].Position = new Vector3(frogRect.MaxMinCorner, 0f);
            vertices[2].Position = new Vector3(frogRect.MinMaxCorner, 0f);
            vertices[3].Position = new Vector3(frogRect.MinCorner, 0f);

            Color color = Color.GreenYellow;
            vertices[0].Color = color;
            vertices[1].Color = color;
            vertices[2].Color = color;
            vertices[3].Color = color;
        }

        private void SetupVertexBuffer(GraphicsDevice device)
        {
            mVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), 4, BufferUsage.WriteOnly);
            device.SetVertexBuffer(mVertexBuffer);
            mVertexBufferData = CreateRectVertices();
            //TODO: Reduce code duplication between renderers!
        }

        private static VertexPositionColor[] CreateRectVertices()
        {
            VertexPositionColor[] vertices = new VertexPositionColor[4];
            return vertices;
        }
    }
}
