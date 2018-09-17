using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ranitas.Core.Render;
using Ranitas.Data;
using Ranitas.Sim;
using System;

namespace Ranitas
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private static readonly int kSuportedPlayers = 4;

        private GraphicsDeviceManager mGraphics;

        private ECSSim mSim;

        private bool[] mSPawnedPlayers = new bool[kSuportedPlayers];
        
        public Game1()
        {
            mGraphics = new GraphicsDeviceManager(this);
            MatchCurrentResolution(mGraphics);
            Content.RootDirectory = "Content";
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 120d);
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
            FrogData frogData = Content.Load<FrogData>("Frog");
            PondData pondData = Content.Load<PondData>("Pond");
            FlyData flyData = Content.Load<FlyData>("Fly"); //WIP TODO WIP

            SetupCamera(mGraphics.GraphicsDevice, pondData);

            PrimitiveRenderer primitiveRenderer = new PrimitiveRenderer();
            primitiveRenderer.Setup(mGraphics.GraphicsDevice);

            System.Diagnostics.Debug.Assert(IsFixedTimeStep);
            RanitasDependencies dependencies = new RanitasDependencies((float)TargetElapsedTime.TotalSeconds, pondData, frogData, primitiveRenderer);
            mSim = new ECSSim(dependencies);
            mSim.Initialize();
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            for (int i = 0; i < kSuportedPlayers; ++i)
            {
                if ((GamePad.GetState(i).Buttons.Back == ButtonState.Pressed) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    Exit();
                }
                if (!mSPawnedPlayers[i])
                {
                    if (GamePad.GetState(i).Buttons.Start == ButtonState.Pressed)
                    {
                        mSim.SpawnPlayer(i);  //TODO: Make a player ID type?
                        mSPawnedPlayers[i] = true;
                    }
                }
            }

            mSim.Update();

            base.Update(gameTime);
        }

        private void SetupCamera(GraphicsDevice device, PondData pondData)
        {
            float ponWidth = pondData.Width;
            float ponHeight = pondData.Height;
            float aspectRatio = device.Adapter.CurrentDisplayMode.AspectRatio;
            BasicEffect effect = new BasicEffect(device);
            effect.VertexColorEnabled = true;
            effect.World = Matrix.CreateTranslation(-ponWidth * 0.5f, -ponHeight * 0.5f, 0f);
            effect.View = Matrix.CreateOrthographic(aspectRatio * ponHeight, ponHeight, -100, 100);
            effect.CurrentTechnique.Passes[0].Apply();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DimGray);
            mSim.Render();
            base.Draw(gameTime);
        }
    }
}
