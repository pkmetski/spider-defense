using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class AboutMenuScreen : MenuScreen
    {
        #region Fields

        private string[] text = new string[]
        { 
            "Created by Plamen Kmetski",
            "p.kmetski@gmail.com",
            "University College Nordjyllands",
            "",
            "All Graphical Content Belongs To Its Respective Owners",
            "",
            "2010"
        };
        Vector2 textPosition = Vector2.Zero;

        //a variable used to scroll the text
        private float positionTimer;

        #endregion

        public AboutMenuScreen()
            : base("About")
        {
            MenuEntry backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            backMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(backMenuEntry);
        }

        #region Draw

        public override void Draw(GameTime gameTime)
        {
            int screenWidth = ScreenManager.GraphicsDeviceManager.PreferredBackBufferWidth;
            int screenHeight = ScreenManager.GraphicsDeviceManager.PreferredBackBufferHeight;
            int rectangleWidth = (int)((2 / 3f) * screenWidth);
            int rectangleHeight = (int)((2 / 3f) * screenHeight);
            int rectangleX = (int)(screenWidth / 2f - rectangleWidth / 2f);
            int rectangleY = (int)(screenHeight / 2f - rectangleHeight / 2f);
            float offset = 10f;

            //initially place the text at the bottom of the rectangle
            if (positionTimer == 0)
                positionTimer = -(rectangleHeight + 5 * offset);

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

            Vector2 dimensions = Vector2.Zero;

            //get the dimensions of the longest string
            foreach (string str in text)
                if (dimensions.X < ScreenManager.Font.MeasureString(str).X)
                    dimensions = ScreenManager.Font.MeasureString(str);

            //calculate the text scale based on the width of the rectangle and the dimensions of the longest string
            float textScale = rectangleWidth / (dimensions.X + 25f * offset);

            float spaceBetweenEntries = rectangleHeight / (text.Length + 1f);

            //set the text position according to the timer's value
            textPosition.Y = -positionTimer;

            for (int i = 0; i < text.Length; i++)
            {
                Vector2 entryDimensions = ScreenManager.Font.MeasureString(text[i]) * textScale;

                textPosition = new Vector2(((rectangleX + (rectangleWidth - entryDimensions.X) / 2f)),
                    textPosition.Y + spaceBetweenEntries);

                if (textPosition.Y + 4 * offset < rectangleY + rectangleHeight &&
                    textPosition.Y + offset / 4 > rectangleY)
                    spriteBatch.DrawString(ScreenManager.Font,
                        text[i],
                        textPosition,
                        Color.White,
                        0,
                        Vector2.Zero,
                        textScale,
                        SpriteEffects.None,
                        0);

            }

            spriteBatch.End();

            //move the text upwards
            positionTimer += .6f;

            base.Draw(gameTime);
        }

        #endregion
    }
}
