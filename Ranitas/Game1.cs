using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Ranitas.Data;
using Ranitas.Pond;

namespace Ranitas
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager mGraphics;

        PondSimState mPond;
        PondRenderer mPondRenderer;
        
        public Game1()
        {
            mGraphics = new GraphicsDeviceManager(this);
            MatchCurrentResolution(mGraphics);
            Content.RootDirectory = "Content";
        }

        private static void MatchCurrentResolution(GraphicsDeviceManager graphics)
        {
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //TODO: Toggling full screen makes debugging a pain
        }

        protected override void Initialize()
        {
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            mGraphics.GraphicsDevice.RasterizerState = rs;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            PondData pondData = Content.Load<PondData>("Pond");
            mPond = new PondSimState(pondData);
            mPondRenderer = new PondRenderer();
            mPondRenderer.Setup(mGraphics.GraphicsDevice, pondData);
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            mPondRenderer.RenderPond(mPond, mGraphics.GraphicsDevice);
            base.Draw(gameTime);
        }
    }
}
