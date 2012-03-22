#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
#endregion

namespace SpiderDefense
{
    /// <summary>
    /// Sample showing how to manage different game states, with transitions
    /// between menu screens, a loading screen, the game itself, and a pause
    /// menu. This main game class is extremely simple: all the interesting
    /// stuff happens in the ScreenManager component.
    /// </summary>
    public class GameStateManagementGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        public bool FULLSCREEN = false;
        int screenWidth = 900;
        int screenHeight = 600;

        KeyboardState keyboardState;
        KeyboardState previousKeyboardState;

        #endregion

        #region Initialization


        /// <summary>
        /// The main game constructor.
        /// </summary>
        public GameStateManagementGame()
        {
            //adjust the update frequency - 40 times per second
            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 40.0f);

            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            Services.AddService(typeof(GraphicsDeviceManager), graphics);
            Services.AddService(typeof(Game), this);
            Services.AddService(typeof(ContentManager), Content);
        }

        protected override void Initialize()
        {
            Window.Title = "Spider Defense";

            // Create the screen manager component.
            screenManager = new ScreenManager(this);
            screenManager.Background = new Random().Next(1,4);

            Components.Add(screenManager);

            // Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);

            //IsMouseVisible = false;

            SwitchFullScreen(true);

            base.Initialize();
        }

        #endregion

        public void SwitchFullScreen(bool initial)
        {
            if (!initial)
                FULLSCREEN = !FULLSCREEN;

            if (!FULLSCREEN)
            {
                graphics.PreferredBackBufferWidth = screenWidth;
                graphics.PreferredBackBufferHeight = screenHeight;
                graphics.IsFullScreen = false;
            }
            else
            {
                graphics.PreferredBackBufferWidth = graphics.GraphicsDevice.DisplayMode.Width;
                graphics.PreferredBackBufferHeight = graphics.GraphicsDevice.DisplayMode.Height;
                graphics.ToggleFullScreen();
            }

            graphics.ApplyChanges();
        }

        #region Update

        protected override void Update(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.F11) &&
                   !previousKeyboardState.IsKeyDown(Keys.F11))
            {
                SwitchFullScreen(false);
            }

            previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        #endregion

        #region Draw

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (GameStateManagementGame game = new GameStateManagementGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
