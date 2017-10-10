using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Ranitas.Data;
using Ranitas.Frog;
using Ranitas.Pond;
using System.Collections.Generic;

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
        List<FrogSimState> mFrogs;
        FrogRenderer mFrogRenderer;
        
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
            FrogData frogData = Content.Load<FrogData>("Frog");

            mPond = new PondSimState(pondData);
            int frogSpawns = pondData.FrogSpawns.Length;
            mFrogs = new List<FrogSimState>(frogSpawns);
            for (int i = 0; i < frogSpawns; ++i)
            {
                mFrogs.Add(mPond.SpawnFrog(frogData, pondData, i));
            }

            mPondRenderer = new PondRenderer();
            mPondRenderer.Setup(mGraphics.GraphicsDevice, pondData);

            mFrogRenderer = new FrogRenderer();
            mFrogRenderer.Setup(mGraphics.GraphicsDevice);
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
            foreach (var frog in mFrogs)
            {
                mFrogRenderer.RenderFrog(frog, mGraphics.GraphicsDevice);
            }
            base.Draw(gameTime);
        }
    }
}
