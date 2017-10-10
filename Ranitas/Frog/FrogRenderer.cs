using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            SetVertices(mVertexBufferData, frog);
            mVertexBuffer.SetData(mVertexBufferData);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        private static void SetVertices(VertexPositionColor[] vertices, FrogSimState frog)
        {
            float halfWidth = frog.Width * 0.5f;
            float halfHeight = frog.Height * 0.5f;
            Vector3 position = new Vector3(frog.Position, 0f);
            vertices[0].Position = new Vector3(halfWidth, halfHeight, 0f) + position;
            vertices[1].Position = new Vector3(halfWidth, -halfHeight, 0f) + position;
            vertices[2].Position = new Vector3(-halfWidth, halfHeight, 0f) + position;
            vertices[3].Position = new Vector3(-halfWidth, -halfHeight, 0f) + position;

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
