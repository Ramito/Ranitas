using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ranitas.Core.Render;
using Ranitas.Data;
using Ranitas.Frog;
using Ranitas.Frog.Sim;
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
        private static readonly int kSuportedPlayers = 4;

        private GraphicsDeviceManager mGraphics;

        //private FrogData mFrogPrototype;
        private float[] mFrogSpawns;

        //private PondSimState mPond;
        private PondRenderer mPondRenderer;

        //private InputProcessor mInputProcessor = new InputProcessor(sSuportedPlayers.Length);

        //private List<FrogSimState> mFrogs;
        private FrogRenderer mFrogRenderer;
        //private FlyRenderer mFlyRenderer;
        private PrimitiveRenderer mPrimitiveRenderer;

        //private RanitasSim mSim;
        private ECSSim mSim;

        private bool[] mSPawnedPlayers = new bool[kSuportedPlayers];
        
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
            FrogData frogData = Content.Load<FrogData>("Frog");
            PondData pondData = Content.Load<PondData>("Pond");
            FlyData flyData = Content.Load<FlyData>("Fly"); //WIP TODO WIP
            mFrogSpawns = pondData.FrogSpawns;

            //mPond = new PondSimState(pondData);
            //mFrogs = new List<FrogSimState>(sSuportedPlayers.Length);

            System.Diagnostics.Debug.Assert(IsFixedTimeStep);
            //mSim = new RanitasSim(flyData, mPond, mFrogs, (float)TargetElapsedTime.TotalSeconds, mPlayerBindings);
            RanitasDependencies dependencies = new RanitasDependencies((float)TargetElapsedTime.TotalSeconds, pondData, frogData);
            mSim = new ECSSim(dependencies);

            mPrimitiveRenderer = new PrimitiveRenderer();
            mPrimitiveRenderer.Setup(mGraphics.GraphicsDevice);

            mPondRenderer = new PondRenderer();
            mPondRenderer.Setup(mGraphics.GraphicsDevice, pondData);

            mFrogRenderer = new FrogRenderer();

            //mFlyRenderer = new FlyRenderer(mSim.FlySim);
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
            //mSim.Update(mInputProcessor.Inputs);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DimGray);

            //WIP WIP WIP
            //mPondRenderer.RenderPond(mPond, mPrimitiveRenderer);
            //WIP WIP WIP

            //foreach (var frog in mFrogs)
            //{
            //    mFrogRenderer.RenderFrog(frog, mPrimitiveRenderer);
            //}
            //mFlyRenderer.Render(mPrimitiveRenderer);
            mPrimitiveRenderer.Render();
            base.Draw(gameTime);
        }
    }
}
