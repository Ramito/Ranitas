using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Core.Render;
using Ranitas.Data;
using Ranitas.Pond;
using Ranitas.Sim;

namespace Ranitas.Render
{
    public sealed class RenderSystem : ISystem
    {
        public RenderSystem(GraphicsDevice graphicsDevice, PondSimState pond, Texture2D frogSprite, SpriteFont uiFont, FrogAnimationData animationData, Effect waterEffect)
        {
            mRenderer = new PrimitiveRenderer();
            mRenderer.Setup(graphicsDevice);

            mFrogRenderer = new FrogRenderer();
            mFrogRenderer.Setup(graphicsDevice, frogSprite, animationData);

            mUIFont = uiFont;

            mPond = pond;
            mDevice = graphicsDevice;
            SetupCamera();

            mUISpriteBatch = new SpriteBatch(mDevice);

            mWaterEffect = waterEffect;
            SetupWaterEffect();
            SetupBasicEffect();
        }

        private PrimitiveRenderer mRenderer;
        private FrogRenderer mFrogRenderer;
        private SpriteFont mUIFont;
        private SpriteBatch mUISpriteBatch;
        private Matrix mCameraMatrix;
        private PondSimState mPond;    //TODO: Make lily pads entities so they can be rendered as the rest!
        private GraphicsDevice mDevice;
        private Effect mWaterEffect;
        private BasicEffect mBasicEffect;

        private void SetupBasicEffect()
        {
            mBasicEffect = new BasicEffect(mDevice);
            mBasicEffect.View = mCameraMatrix;
            mBasicEffect.VertexColorEnabled = true;
            mBasicEffect.World = Matrix.Identity;
            mBasicEffect.Alpha = 1;
            mBasicEffect.TextureEnabled = false;
        }

        private void SetupWaterEffect()
        {
            Vector4 surfaceColor = Color.LightCyan.ToVector4();
            surfaceColor.W = 0.05f;
            Vector4 bottomColor = Color.MidnightBlue.ToVector4();
            bottomColor *= 0.35f;
            bottomColor.W = 0.99f;
            mWaterEffect.Parameters["SurfaceColor"].SetValue(surfaceColor);
            mWaterEffect.Parameters["BottomColor"].SetValue(bottomColor);
            mWaterEffect.Parameters["WorldViewProjection"].SetValue(mCameraMatrix);
        }

        private struct ColoredRectSlice
        {
            public SliceRequirementOutput<Rect> Rect;
            public SliceRequirementOutput<Color> Color;
        }
        private ColoredRectSlice mColoredRectSlice;

        private struct FrogRectSlice
        {
            public SliceRequirementOutput<Rect> Rect;
            public SliceRequirementOutput<AnimationState> Animation;
        }
        private FrogRectSlice mFrogRectSlice;

        private struct PlayerSlice
        {
            public SliceRequirementOutput<Player> Player;
            public SliceRequirementOutput<Score> Score;
        }
        private PlayerSlice mPlayerSlice;

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mColoredRectSlice);
            registry.SetupSlice(ref mFrogRectSlice);
            registry.SetupSlice(ref mPlayerSlice);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            mDevice.Clear(Color.SkyBlue);

            RenderLiliesAndWaterBackground();

            int frogCount = mFrogRectSlice.Rect.Count;
            for (int i = 0; i < frogCount; ++i)
            {
                mFrogRenderer.PushFrog(mFrogRectSlice.Rect[i], mFrogRectSlice.Animation[i]);
            }
            mFrogRenderer.Render(mCameraMatrix, mDevice);

            int rectCount = mColoredRectSlice.Rect.Count;
            for (int i = 0; i < rectCount; ++i)
            {
                mRenderer.PushRect(mColoredRectSlice.Rect[i], mColoredRectSlice.Color[i]);
            }
            mRenderer.RenderAndFlush(mDevice, mBasicEffect);

            RenderWater();
            RenderLetterBox();
            RenderUI();
        }

        private void RenderLiliesAndWaterBackground()
        {
            mRenderer.PushRect(new Rect(Vector2.Zero, new Vector2(mPond.Width, mPond.WaterLevel)), Color.CadetBlue);
            foreach (var lily in mPond.Lilies)
            {
                mRenderer.PushRect(lily.Rect, Color.LawnGreen);
            }
            mRenderer.RenderAndFlush(mDevice, mBasicEffect);
        }

        private void RenderWater()
        {
            float width = mPond.Width;
            float height = mPond.WaterLevel;

            //Red channel is horizontal distance, Green channel is depth
            mRenderer.StartShape();
            mRenderer.ShapeVertex(new Vector2(width, height), new Color(1.0f, 0.0f, 0.0f, 1.0f));
            mRenderer.ShapeVertex(new Vector2(width, 0.0f), new Color(1.0f, 1.0f, 0.0f, 1.0f));
            mRenderer.ShapeVertex(new Vector2(0.0f, height), new Color(0.0f, 0.0f, 0.0f, 1.0f));
            mRenderer.ShapeVertex(new Vector2(0.0f, 0.0f), new Color(0.0f, 1.0f, 0.0f, 1.0f));
            mRenderer.EndShape();

            mRenderer.RenderAndFlush(mDevice, mWaterEffect);
        }

        private void RenderLetterBox()
        {
            float width = MathHelper.Min(mDevice.DisplayMode.Width, mPond.Width);
            float aspectRatio = mDevice.Adapter.CurrentDisplayMode.AspectRatio;
            float height = width / aspectRatio;
            Rect bottomLetterBox = new Rect(new Vector2(0.0f, (mPond.Height - height) * 0.5f), new Vector2(width, 0.0f));
            mRenderer.PushRect(bottomLetterBox, Color.Black);
            Rect topLetterBox = bottomLetterBox.Translated(new Vector2(0.0f, mPond.Height + bottomLetterBox.Height));
            mRenderer.PushRect(topLetterBox, Color.Black);
            mRenderer.RenderAndFlush(mDevice, mBasicEffect);
        }

        private void RenderUI()
        {
            float playerAreaWidth = (float)(mDevice.DisplayMode.Width) / 4f;
            int playerCount = mPlayerSlice.Player.Count;
            mUISpriteBatch.Begin(depthStencilState: DepthStencilState.Default);
            for (int i = 0; i < playerCount; ++i)
            {
                float xPosition = mPlayerSlice.Player[i].Index * playerAreaWidth;
                Vector2 position = new Vector2(xPosition + playerAreaWidth * 0.25f, playerAreaWidth * 0.25f);
                //TODO: Can we avoid string allocations... does it even matter?
                string scoreString = string.Format("Score: {0}", mPlayerSlice.Score[i].Value);
                mUISpriteBatch.DrawString(mUIFont, scoreString, position, Color.White);
            }
            mUISpriteBatch.End();
        }

        private void SetupCamera()
        {
            float width = MathHelper.Min(mDevice.DisplayMode.Width, mPond.Width);
            float aspectRatio = mDevice.Adapter.CurrentDisplayMode.AspectRatio;
            float height = width/aspectRatio;
            Matrix translation = Matrix.CreateTranslation(-mPond.Width * 0.5f, -mPond.Height * 0.5f, 0f);
            Matrix projectionMatrix = Matrix.CreateOrthographic(width, height, -100, 100);
            mCameraMatrix = translation * projectionMatrix;
        }
    }
}
