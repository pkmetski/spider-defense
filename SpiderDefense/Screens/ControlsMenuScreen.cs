#region File Description
//-----------------------------------------------------------------------------
// ControlsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SpiderDefense
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class ControlsMenuScreen : MenuScreen
    {
        #region Fields

        private string[] options;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public ControlsMenuScreen()
            : base("Controls")
        {

            //initialize options array
            options = new string[]
            {
             "Up/Down/Left/Right Arrow - move around the map",
             "Left Ctr + Up/Down/Left/Right Arrow - change view angle",
             "Mouse Wheel - Zoom In/Out",
             "F11 - switch between fullscreen and windowed mode",
             "P/Pause - pause the game"
            };

            MenuEntry backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            backMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(backMenuEntry);
        }


        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {
            int screenWidth = ScreenManager.GraphicsDeviceManager.PreferredBackBufferWidth;
            int screenHeight = ScreenManager.GraphicsDeviceManager.PreferredBackBufferHeight;
            int rectangleWidth = (int)((2 / 3f) * screenWidth);
            int rectangleHeight = (int)((2 / 3f) * screenHeight);
            int rectangleX = (int)(screenWidth / 2f - rectangleWidth / 2f);
            int rectangleY = (int)(screenHeight / 2f - rectangleHeight / 2f);

            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
                rectangleX -= (int)(transitionOffset * 256f);
            else
                rectangleX += (int)(transitionOffset * 512f);


            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            ContentManager content = (ContentManager)ScreenManager.Game.Services.GetService(typeof(ContentManager));

            Rectangle rec = new Rectangle(rectangleX, rectangleY, rectangleWidth, rectangleHeight);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            spriteBatch.Draw(content.Load<Texture2D>(@"Textures\blank"), rec, new Color(0, 0, 0, 150));

            float offset = 10f;
            Vector2 dimensions = Vector2.Zero;

            //get the dimensions of the longest string
            foreach (string str in options)
                if (dimensions.X < ScreenManager.Font.MeasureString(str).X)
                    dimensions = ScreenManager.Font.MeasureString(str);

            //calculate the text dimensions based on the width of the rectangle and the length of the longest string
            float textScale = rectangleWidth / (dimensions.X + 2 * offset);

            float spaceBetweenEntries = (rectangleHeight / (options.Length + 1f));

            Vector2 textPosition = new Vector2(rectangleX + offset, rectangleY + spaceBetweenEntries - 2 * offset);

            for (int i = 0; i < options.Length; i++)
            {
                spriteBatch.DrawString(ScreenManager.Font,
                    options[i],
                    textPosition,
                    Color.White,
                    0,
                    Vector2.Zero,
                    textScale,
                    SpriteEffects.None,
                    0);

                textPosition.Y += spaceBetweenEntries;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Handle Input




        #endregion
    }
}
