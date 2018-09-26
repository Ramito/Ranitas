using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Core.Render;
using Ranitas.Pond;
using Ranitas.Sim;

namespace Ranitas.Render
{
    public sealed class RenderSystem : ISystem
    {
        public RenderSystem(GraphicsDevice graphicsDevice, PondSimState pond, Texture2D frogSprite)
        {
            mRenderer = new PrimitiveRenderer();
            mRenderer.Setup(graphicsDevice);

            mFrogRenderer = new FrogRenderer();
            mFrogRenderer.Setup(graphicsDevice, frogSprite);

            mPond = pond;
            mDevice = graphicsDevice;
            SetupCamera(graphicsDevice, pond);
        }

        private void SetupDevice(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        private PrimitiveRenderer mRenderer;
        private FrogRenderer mFrogRenderer;
        private Matrix mCameraMatrix;
        private PondSimState mPond;    //TODO: Make lily pads entities so they can be rendered as the rest!
        private GraphicsDevice mDevice;

        private struct ColoredRectSlice
        {
            public SliceRequirementOutput<Rect> Rect;
            public SliceRequirementOutput<Color> Color;
            public SliceExclusion<FrogControlState> NotFrogs;
        }
        ColoredRectSlice mColoredRectSlice;

        private struct FrogRectSlice
        {
            public SliceEntityOutput Entity;
            public SliceRequirementOutput<Rect> Rect;
            public SliceRequirementOutput<Facing> Facing;
            public SliceRequirement<FrogControlState> IsFrog;
        }
        FrogRectSlice mFrogRectSlice;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mColoredRectSlice);
            registry.SetupSlice(ref mFrogRectSlice);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            mDevice.Clear(Color.DimGray);
            int rectCount = mColoredRectSlice.Rect.Count;
            for (int i = 0; i < rectCount; ++i)
            {
                mRenderer.PushRect(mColoredRectSlice.Rect[i], mColoredRectSlice.Color[i]);
            }
            mRenderer.Render(mCameraMatrix, mDevice);
            RenderPond();

            int frogCount = mFrogRectSlice.Rect.Count;
            for (int i = 0; i < frogCount; ++i)
            {
                mFrogRenderer.PushFrog(mFrogRectSlice.Rect[i], mFrogRectSlice.Facing[i].CurrentFacing, !registry.HasComponent<Landed>(mFrogRectSlice.Entity[i]));
            }
            mFrogRenderer.Render(mCameraMatrix, mDevice);
        }

        private void RenderPond()
        {
            foreach (var lily in mPond.Lilies)
            {
                mRenderer.PushRect(lily.Rect, Color.Green);
            }
            RenderWater();
        }

        private void RenderWater()
        {
            int wide = (int)mPond.Width;
            Rect waterRect = new Rect(new Vector2(-wide, 0f), new Vector2(wide, mPond.WaterLevel));
            Color waterColor = Color.DarkBlue;
            waterColor.A = 1;
            mRenderer.PushRect(waterRect, waterColor);
        }

        private void SetupCamera(GraphicsDevice device, PondSimState pond)
        {
            float pondWidth = pond.Width;
            float pondHeight = pond.Height;
            float aspectRatio = device.Adapter.CurrentDisplayMode.AspectRatio;
            Matrix translation = Matrix.CreateTranslation(-aspectRatio * pondHeight * 0.5f, -pondHeight * 0.5f, 0f);
            Matrix projectionMatrix = Matrix.CreateOrthographic(aspectRatio * pondHeight, pondHeight, -100, 100);
            mCameraMatrix = translation * projectionMatrix;
        }
    }
}
