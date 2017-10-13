﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Ranitas.Data;
using Ranitas.Frog;
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
        private List<FrogSimState> mFrogs;
        private FrogRenderer mFrogRenderer;

        private RanitasSim mSim;

        private FrogInput[] mRegisteredPlayers = new FrogInput[sSuportedPlayers.Length];
        
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
            mFrogSpawns = pondData.FrogSpawns;

            mPond = new PondSimState(pondData);
            mFrogs = new List<FrogSimState>(sSuportedPlayers.Length);

            System.Diagnostics.Debug.Assert(IsFixedTimeStep);
            mSim = new RanitasSim(mPond, mFrogs, (float)TargetElapsedTime.TotalSeconds);

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
            foreach (var playerIndex in sSuportedPlayers)
            {
                if (mRegisteredPlayers[playerIndex] == null)
                {
                    if (GamePad.GetState(playerIndex).Buttons.Start == ButtonState.Pressed)
                    {
                        FrogSimState frog = mPond.SpawnFrog(mFrogPrototype, mFrogSpawns, playerIndex);
                        mFrogs.Add(frog);
                        mRegisteredPlayers[playerIndex] = new FrogInput(frog);
                    }
                }
                else
                {
                    bool jumpButton = (GamePad.GetState(playerIndex).Buttons.A == ButtonState.Pressed);
                    Vector2 controller = GamePad.GetState(playerIndex).ThumbSticks.Left;
                    FrogInput frogInput = mRegisteredPlayers[playerIndex];
                    bool previousState = frogInput.JumpButtonState;
                    frogInput.Update(controller, jumpButton);
                    if (previousState != frogInput.JumpButtonState)
                    {
                        GamePad.SetVibration(playerIndex, 1f, 1f);
                    }
                    else
                    {
                        GamePad.SetVibration(playerIndex, 0f, 0f);
                    }
                }
                if ((GamePad.GetState(playerIndex).Buttons.Back == ButtonState.Pressed) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    Exit();
                }
            }

            mSim.Update();
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
