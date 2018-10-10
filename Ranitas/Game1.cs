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
            FlyData flyData = Content.Load<FlyData>("Fly");
            FlyDirectionChangeData changeData = Content.Load<FlyDirectionChangeData>("DirectionChange");
            FlyNoiseData noiseData = Content.Load<FlyNoiseData>("FlyNoise");
            FrogAnimationData animationData = Content.Load<FrogAnimationData>("FrogAnimation");
            Texture2D frogSprite = Content.Load<Texture2D>("Ranita");

            SpriteFont uiFont = Content.Load<SpriteFont>("GameUI");

            System.Diagnostics.Debug.Assert(IsFixedTimeStep);
            RanitasDependencies dependencies = new RanitasDependencies((float)TargetElapsedTime.TotalSeconds, pondData, frogData, flyData, changeData, noiseData, animationData, frogSprite, mGraphics.GraphicsDevice, uiFont);
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
                        mSim.SpawnPlayer(i);
                        mSPawnedPlayers[i] = true;
                    }
                }
            }

            mSim.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            mSim.Render();
            base.Draw(gameTime);
        }
    }
}
