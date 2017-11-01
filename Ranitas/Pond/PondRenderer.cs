using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ranitas.Core;
using Ranitas.Core.Render;
using Ranitas.Data;

namespace Ranitas.Pond
{
    public sealed class PondRenderer
    {
        private BasicEffect mEffect;    //TODO: Not sure this guy should be here!

        public void Setup(GraphicsDevice device, PondData pondData)
        {
            SetupCamera(device, pondData);
        }

        private void SetupCamera(GraphicsDevice device, PondData pondData)
        {
            float ponWidth = pondData.Width;
            float ponHeight = pondData.Height;
            float aspectRatio = device.Adapter.CurrentDisplayMode.AspectRatio;
            mEffect = new BasicEffect(device);
            mEffect.VertexColorEnabled = true;
            mEffect.World = Matrix.CreateTranslation(-ponWidth * 0.5f, -ponHeight * 0.5f, 0f);
            mEffect.View = Matrix.CreateOrthographic(aspectRatio * ponHeight, ponHeight, -100, 100);
            mEffect.CurrentTechnique.Passes[0].Apply();
        }

        public void RenderPond(PondSimState pond, PrimitiveRenderer renderer)
        {
            DrawWater(renderer, pond, renderer.Device.Adapter.CurrentDisplayMode.Width);
            foreach (var lily in pond.Lilies)
            {
                renderer.PushRect(lily.Rect, Color.Green);
            }
        }

        private static void DrawWater(PrimitiveRenderer renderer, PondSimState pond, float screenWidth)
        {
            Rect waterRect = new Rect(new Vector2(-screenWidth, 0f), new Vector2(screenWidth, pond.WaterLevel));
            renderer.PushRect(waterRect, Color.DarkBlue);
        }
    }
}
