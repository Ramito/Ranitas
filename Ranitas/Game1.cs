using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Ranitas.Data;
using Ranitas.Frog;
using Ranitas.Frog.Sim;
using Ranitas.Input;
using Ranitas.Insects;
using Ranitas.Pond;
using Ranitas.Sim;
using System.Collections.Generic;

namespace Ranitas
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private static readonly int[] sSuportedPlayers = new[] { 0, 1, 2, 3 };

        private GraphicsDeviceManager mGraphics;

        private FrogData mFrogPrototype;
        private float[] mFrogSpawns;

        private PondSimState mPond;
        private PondRenderer mPondRenderer;

        private InputProcessor mInputProcessor = new InputProcessor(sSuportedPlayers.Length);

        private List<FrogSimState> mFrogs;
        private FrogRenderer mFrogRenderer;
        private FlyRenderer mFlyRenderer;

        private RanitasSim mSim;

        private PlayerBinding[] mPlayerBindings = new PlayerBinding[sSuportedPlayers.Length];
        
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
            mFrogPrototype = Content.Load<FrogData>("Frog");
            PondData pondData = Content.Load<PondData>("Pond");
            FlyData flyData = Content.Load<FlyData>("Fly");
            mFrogSpawns = pondData.FrogSpawns;

            mPond = new PondSimState(pondData);
            mFrogs = new List<FrogSimState>(sSuportedPlayers.Length);

            System.Diagnostics.Debug.Assert(IsFixedTimeStep);
            mSim = new RanitasSim(flyData, mPond, mFrogs, (float)TargetElapsedTime.TotalSeconds);

            mPondRenderer = new PondRenderer();
            mPondRenderer.Setup(mGraphics.GraphicsDevice, pondData);

            mFrogRenderer = new FrogRenderer();
            mFrogRenderer.Setup(mGraphics.GraphicsDevice);

            mFlyRenderer = new FlyRenderer(mSim.FlySim);
            mFlyRenderer.Setup(mGraphics.GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            foreach (var playerIndex in sSuportedPlayers)
            {
                if (mPlayerBindings[playerIndex] == null)
                {
                    if (GamePad.GetState(playerIndex).Buttons.Start == ButtonState.Pressed)
                    {
                        FrogSimState frog = mPond.SpawnFrog(mFrogPrototype, mFrogSpawns, playerIndex);
                        mFrogs.Add(frog);
                        mPlayerBindings[playerIndex] = new PlayerBinding(playerIndex, frog);
                    }
                }
                else
                {
                    mInputProcessor.ProcessInput(playerIndex);
                }
                if ((GamePad.GetState(playerIndex).Buttons.Back == ButtonState.Pressed) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    Exit();
                }
            }

            mSim.Update(mPlayerBindings, mInputProcessor.Inputs);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DimGray);
            mPondRenderer.RenderPond(mPond, mGraphics.GraphicsDevice);
            foreach (var frog in mFrogs)
            {
                mFrogRenderer.RenderFrog(frog, mGraphics.GraphicsDevice);
            }
            mFlyRenderer.Render(mGraphics.GraphicsDevice);
            base.Draw(gameTime);
        }
    }
}
