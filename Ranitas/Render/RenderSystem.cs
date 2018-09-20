using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Core.Render;
using Ranitas.Pond;
using Ranitas.Sim;

namespace Ranitas.Render
{
    public sealed class RenderSystem : ISystem
    {
        public RenderSystem(PrimitiveRenderer renderer, PondSimState pond)
        {
            mRenderer = renderer;
            mPond = pond;
        }

        private PrimitiveRenderer mRenderer;
        private PondSimState mPond;    //TODO: Make lily pads entities so they can be rendered as the rest!

        private struct ColoredRectSlice
        {
            public SliceRequirementOutput<Rect> Rect;
            public SliceRequirementOutput<Color> Color;
        }
        ColoredRectSlice mColoredRectSlice;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mColoredRectSlice);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            RenderPond();
            int count = mColoredRectSlice.Rect.Count;
            for (int i = 0; i < count; ++i)
            {
                mRenderer.PushRect(mColoredRectSlice.Rect[i], mColoredRectSlice.Color[i]);
            }
            mRenderer.Render();
        }

        private void RenderPond()
        {
            RenderWater();
            foreach (var lily in mPond.Lilies)
            {
                mRenderer.PushRect(lily.Rect, Color.Green);
            }
        }

        private void RenderWater()
        {
            int wide = (int)mPond.Width;
            Rect waterRect = new Rect(new Vector2(-wide, 0f), new Vector2(wide, mPond.WaterLevel));
            mRenderer.PushRect(waterRect, Color.DarkBlue);
        }
    }
}
