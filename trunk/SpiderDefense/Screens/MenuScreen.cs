#region File Description
//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SpiderDefense
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    abstract class MenuScreen : GameScreen
    {
        #region Fields

        List<MenuEntry> menuEntries = new List<MenuEntry>();
        int selectedEntry = 0;
        string menuTitle;
        Vector2 shadowOffset = new Vector2(1.4f, 1.0f);

        #endregion

        #region Properties


        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<MenuEntry> MenuEntries
        {
            get { return menuEntries; }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen(string menuTitle)
        {
            this.menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            // Move to the previous menu entry?
            if (input.IsMenuUp(ControllingPlayer))
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = menuEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (input.IsMenuDown(ControllingPlayer))
            {
                selectedEntry++;

                if (selectedEntry >= menuEntries.Count)
                    selectedEntry = 0;
            }

            // Accept or cancel the menu? We pass in our ControllingPlayer, which may
            // either be null (to accept input from any player) or a specific index.
            // If we pass a null controlling player, the InputState helper returns to
            // us which player actually provided the input. We pass that through to
            // OnSelectEntry and OnCancel, so they can tell which player triggered them.
            PlayerIndex playerIndex;

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex))
            {
                OnSelectEntry(selectedEntry, playerIndex);
            }
            else if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
            {
                OnCancel(playerIndex);
            }
        }


        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
        {
            menuEntries[selectedEntry].OnSelectEntry(playerIndex);
        }


        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel(PlayerIndex playerIndex)
        {
            ExitScreen();
        }


        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry);

                menuEntries[i].Update(this, isSelected, gameTime);
            }
        }


        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;
            GraphicsDeviceManager graphics = ScreenManager.GraphicsDeviceManager;

            int screenWidth = graphics.PreferredBackBufferWidth;
            int screenHeight = graphics.PreferredBackBufferHeight;

            float scale = screenWidth / 900f;

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            for (int i = 0; i < menuEntries.Count; i++)
            {
                Vector2 entryDimensions = font.MeasureString(menuEntries[i].Text) * scale;

                if (menuEntries.Count > 1)
                    menuEntries[i].Position = new Vector2((screenWidth - entryDimensions.X) / 2,
                            (screenHeight - entryDimensions.Y) / 2);
                else
                    menuEntries[i].Position = new Vector2((screenWidth - entryDimensions.X) / 2,
                screenHeight - screenHeight * (1 / 14f));

                if (i > 0)
                    menuEntries[i].Position = new Vector2(menuEntries[i].Position.X,
                        menuEntries[i - 1].Position.Y + menuEntries[i].GetHeight(this) * scale);


                if (ScreenState == ScreenState.TransitionOn)
                    menuEntries[i].Position = new Vector2(menuEntries[i].Position.X - transitionOffset * 256,
                        menuEntries[i].Position.Y);
                else
                    menuEntries[i].Position = new Vector2(menuEntries[i].Position.X + transitionOffset * 512,
                        menuEntries[i].Position.Y);
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            // Draw each menu entry in turn.
            for (int i = 0; i < menuEntries.Count; i++)
            {

                MenuEntry menuEntry = menuEntries[i];

                bool isSelected = IsActive && (i == selectedEntry);

                //draw the shadow
                menuEntry.Draw(this, menuEntries[i].Position + shadowOffset, isSelected, true, scale, gameTime);

                //draw the entry
                menuEntry.Draw(this, menuEntries[i].Position, isSelected, false, scale, gameTime);
            }

            // Draw the menu title.
            float titleScale = (screenWidth / 900f) * 1.15f;
            Vector2 titleDimensions = font.MeasureString(menuTitle) * titleScale;
            Vector2 titlePosition = new Vector2((screenWidth - titleDimensions.X) / 2, screenHeight * (1 / 14f));
            Color titleColor = new Color(192, 192, 192, TransitionAlpha);

            titlePosition.Y -= transitionOffset * 100;

            //draw title shadow
            spriteBatch.DrawString(font, menuTitle, titlePosition + shadowOffset, Color.Black, 0,
                                   Vector2.Zero, titleScale, SpriteEffects.None, 0);

            //draw title
            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   Vector2.Zero, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion
    }
}
