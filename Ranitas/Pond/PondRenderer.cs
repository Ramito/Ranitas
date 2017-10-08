using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ranitas.Data;

namespace Ranitas.Pond
{
    public sealed class PondRenderer
    {
        private BasicEffect mEffect;
        private VertexBuffer mVertexBuffer;
        private VertexPositionColor[] mVertexBufferData;

        public void Setup(GraphicsDevice device, PondData pondData)
        {
            SetupCamera(device, pondData);
            SetupVertexBuffer(device);
        }

        private void SetupCamera(GraphicsDevice device, PondData pondData)
        {
            float ponWidth = pondData.Width;
            float ponHeight = pondData.Height;
            mEffect = new BasicEffect(device);
            mEffect.VertexColorEnabled = true;
            mEffect.World = Matrix.CreateTranslation(-ponWidth * 0.5f, -ponHeight * 0.5f, 0f);
            mEffect.View = Matrix.CreateOrthographic(ponWidth, ponHeight, -100, 100);
            mEffect.CurrentTechnique.Passes[0].Apply();
        }

        private void SetupVertexBuffer(GraphicsDevice device)
        {
            mVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), 4, BufferUsage.WriteOnly);
            device.SetVertexBuffer(mVertexBuffer);
            mVertexBufferData = CreateRectVertices();
        }

        public void RenderPond(PondSimState pond, GraphicsDevice device)
        {
            foreach (var lilly in pond.Lillies)
            {
                SetVertices(mVertexBufferData, lilly);
                mVertexBuffer.SetData(mVertexBufferData);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

        private static void SetVertices(VertexPositionColor[] vertices, LillyPadSimState lilly)
        {
            float halfWidth = lilly.Width * 0.5f;
            float halfHeight = lilly.Height * 0.5f;
            Vector3 position = new Vector3(lilly.Position, 0f);
            vertices[0].Position = new Vector3(halfWidth, halfHeight, 0f) + position;
            vertices[1].Position = new Vector3(halfWidth, -halfHeight, 0f) + position;
            vertices[2].Position = new Vector3(-halfWidth, halfHeight, 0f) + position;
            vertices[3].Position = new Vector3(-halfWidth, -halfHeight, 0f) + position;

            Color color = Color.Green;
            vertices[0].Color = color;
            vertices[1].Color = color;
            vertices[2].Color = color;
            vertices[3].Color = color;
        }

        private static VertexPositionColor[] CreateRectVertices()
        {
            VertexPositionColor[] vertices = new VertexPositionColor[4];
            return vertices;
        }
    }
}
