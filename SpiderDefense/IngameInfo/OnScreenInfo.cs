using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class OnScreenInfo
    {
        private GraphicsDeviceManager graphics;
        private Camera camera;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private ContentManager content;
        private Vector2 shadowOffset = new Vector2(1.0f, .75f);
        public Rectangle upgradeButton;
        private Texture2D texture;

        public OnScreenInfo(GraphicsDeviceManager graphics, Camera camera, SpriteBatch spriteBatch, ContentManager content)
        {
            this.graphics = graphics;
            this.camera = camera;
            this.spriteBatch = spriteBatch;
            this.content = content;
            font = content.Load<SpriteFont>(@"Fonts\Courier New");
            texture = content.Load<Texture2D>(@"Textures\blank");
        }

        private void DrawText(string text, Vector2 position, float scale, Color color)
        {
            spriteBatch.DrawString(font,
                    text,
                    position,
                    color,
                    0.0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0.0f);
        }

        public void DrawInfo(string[] info)
        {
            info = info.Reverse().ToArray();

            int screenHeight = graphics.PreferredBackBufferHeight;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            for (int i = 0; i < info.Length; i++)
            {
                //draw the shadow
                DrawText(info[i], new Vector2(8, screenHeight - i * 20 - 30) + shadowOffset, .5f, Color.Black);

                //draw text
                DrawText(info[i], new Vector2(8, screenHeight - i * 20 - 30), .5f, Color.White);
            }

            spriteBatch.End();
        }

        private Vector3 GetScreenSpace(Matrix world)
        {
            // calculate screenspace of text3d space position
            return graphics.GraphicsDevice.Viewport.Project(Vector3.Zero,
                                                             camera.Projection,
                                                             camera.View,
                                                             world);
        }

        public void DrawModelTopInfo(MapModel mapModel)
        {
            string info = mapModel.ModelTopInfo;

            if (mapModel.Font != null)
                font = mapModel.Font;

            float textScale = 22 / mapModel.DistanceFromCamera;

            Vector3 screenSpace = GetScreenSpace(Matrix.CreateTranslation(mapModel.Position + mapModel.TopInfoOffset));

            Vector2 stringCenter = font.MeasureString(info) * 0.5f * textScale;

            Vector2 textPosition;
            textPosition.X = (int)(screenSpace.X - stringCenter.X);
            textPosition.Y = (int)(screenSpace.Y - stringCenter.Y);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            //draw shadow
            DrawText(info, textPosition + shadowOffset, textScale, Color.Black);

            // draw text
            DrawText(info, textPosition, textScale, Color.Yellow);

            spriteBatch.End();
        }

        public void DrawHealthBar(DynamicModel dynamicModel)
        {
            int healthBarWidth = 3000;
            int healthBarHeight = 380;

            if (dynamicModel.Font != null)
                font = dynamicModel.Font;

            Vector3 screenSpace = GetScreenSpace(Matrix.CreateTranslation(dynamicModel.Position));


            Vector2 offset = (dynamicModel.HealthBarOffset * 600) / dynamicModel.DistanceFromCamera;

            #region Background Rectangle

            int backgroundWidth = (int)(healthBarWidth / (dynamicModel.DistanceFromCamera));
            int backgroundHeight = (int)(healthBarHeight / (dynamicModel.DistanceFromCamera));


            Vector2 healthBarCenter = new Vector2(backgroundWidth, backgroundHeight) * 0.5f;

            Vector2 backgroundPosition;
            backgroundPosition.X = (int)(screenSpace.X - healthBarCenter.X) + offset.X;
            backgroundPosition.Y = (int)(screenSpace.Y - healthBarCenter.Y) + offset.Y;


            Rectangle backgroundRectangle = new Rectangle((int)backgroundPosition.X,
                (int)backgroundPosition.Y,
                backgroundWidth,
                backgroundHeight);

            #endregion

            #region Foreground Rectangle

            int foregroundWidth = backgroundWidth - 2;
            int foregroundHeight = backgroundHeight - 2;

            Vector2 foregroundPosition;
            foregroundPosition.X = backgroundPosition.X + 1;
            foregroundPosition.Y = backgroundPosition.Y + 1;

            if (dynamicModel.DistanceFromCamera > healthBarHeight / 3 - 1)
            {
                foregroundWidth = backgroundWidth - 1;
                foregroundHeight = backgroundHeight - 1;
                foregroundPosition = backgroundPosition;
            }

            Rectangle foregroundRectangle = new Rectangle((int)foregroundPosition.X,
                (int)foregroundPosition.Y,
                foregroundWidth,
                foregroundHeight);

            #endregion

            #region Health Rectangle

            int healthWidth = foregroundWidth;
            int healthHeight = foregroundHeight;

            healthWidth = (int)(foregroundWidth * (dynamicModel.CurrentHP / dynamicModel.TotalHP));

            Vector2 healthPosition;
            healthPosition.X = foregroundPosition.X;
            healthPosition.Y = foregroundPosition.Y;

            Rectangle healthRectangle = new Rectangle((int)healthPosition.X,
                (int)healthPosition.Y,
                healthWidth,
                healthHeight);

            #endregion

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            //draw background
            if (dynamicModel.DistanceFromCamera < healthBarHeight / 3)
                spriteBatch.Draw(texture, backgroundRectangle, Color.Black);

            //draw foreground
            spriteBatch.Draw(texture, foregroundRectangle, new Color(211, 211, 211, 220));

            //draw remaining health
            spriteBatch.Draw(texture, healthRectangle, Color.Red);

            spriteBatch.End();
        }

        public void DrawUpgradeRectangle(Tower tower)
        {
            int offset = 40;
            int bckgWidth = (int)(graphics.PreferredBackBufferWidth / 4);
            int bckgHeight = (int)(graphics.PreferredBackBufferHeight - 2 * offset);
            int bckgX = graphics.PreferredBackBufferWidth - bckgWidth - offset;
            int bckgY = (int)((graphics.PreferredBackBufferHeight - bckgHeight) / 2);
            float textScale = 1.0f;
            float textLeftOffset = 6f;
            float upgrTextScale = 1.0f;
            string upgradeText = "Upgrade";
            float distanceBetweenLines = 1.0f;
            TowerCharacteristics afterUpgr = tower.AfterUpgrade();

            string[] text = new string[6];
            text[0] = "Next Level: " + afterUpgr.CurrentLevel;
            text[1] = "Upgrade Cost: " + afterUpgr.UpgradeFee + " points";
            text[2] = "Range: " + afterUpgr.Range.ToString("#0");
            text[3] = "Damage: " + afterUpgr.Damage.ToString("#0.0");
            text[4] = "Arrow Speed: " + afterUpgr.ArrowSpeed.ToString("#0.0000");
            text[5] = "Shooting Frequency: " + (afterUpgr.ShootFrequency / 1000).ToString("#0.00");

            string longest = "";
            foreach (string str in text)
                if (longest.Length < str.Length)
                    longest = str + textLeftOffset;
            textScale = bckgWidth / 478f;
            Vector2 textDimensions = font.MeasureString(longest) * textScale;

            Rectangle rec = new Rectangle(bckgX, bckgY, bckgWidth, bckgHeight);

            int btnWidth = bckgHeight / 4;
            int btnHeight = bckgWidth / 5;
            int btnX = (int)(rec.X + ((bckgWidth - btnWidth) / 2f));
            int btnY = (int)(rec.Y + bckgHeight - (btnHeight + bckgHeight / 20f));

            upgrTextScale = btnWidth / 193f;
            Vector2 upgrTextDimensions = font.MeasureString(upgradeText) * upgrTextScale;

            upgradeButton = new Rectangle(btnX, btnY, btnWidth, btnHeight);

            distanceBetweenLines = (bckgHeight - (btnHeight + offset)) / text.Length;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            spriteBatch.Draw(texture, rec, new Color(0, 0, 0, 100));

            //draw tower upgradeable characteristics
            for (int i = 0; i < text.Length; i++)
            {
                DrawText(text[i],
                    new Vector2(rec.X + textLeftOffset, rec.Y + i * distanceBetweenLines + textLeftOffset),
                    textScale,
                    Color.Yellow);
            }

            //draw upgrade button
            spriteBatch.Draw(texture, upgradeButton, new Color(0, 0, 0, 190));

            //draw upgrade button text
            DrawText(upgradeText,
                new Vector2((int)(upgradeButton.X + (btnWidth - upgrTextDimensions.X) / 2),
                    (int)(upgradeButton.Y + (btnHeight - upgrTextDimensions.Y) / 2)),
                    upgrTextScale,
                    Color.White);

            spriteBatch.End();
        }

        public void DrawLevelInfo(LevelInfo info)
        {
            int screenWidth = graphics.PreferredBackBufferWidth;
            int screenHeight = graphics.PreferredBackBufferHeight;
            int offset = 5;
            float fontScale = .6f;
            Color textColor = Color.Yellow;

            string spiderCountText = "Spiders on the Field: " + info.EnemyCount;
            Vector2 spiderCountDimensions = font.MeasureString(spiderCountText) * fontScale;

            string currentLevelText = "Level: " + info.CurrentGameLevel;
            Vector2 currentLevelDimensions = font.MeasureString(currentLevelText) * fontScale;

            string playerPointsText = "Points: " + info.PlayerPoints.ToString();
            Vector2 playerPointsDimensions = font.MeasureString(playerPointsText) * fontScale;

            string missedEnemiesCount = "Missed Spiders: " + info.MissedEnemies + "/" + info.MaxMissedEnemies;

            Vector2 enemyCountPosition = new Vector2(screenWidth - spiderCountDimensions.X - offset, offset);
            Vector2 currentLevelPosition = new Vector2(offset, offset);
            Vector2 pointsPosition = new Vector2(currentLevelPosition.X + currentLevelDimensions.X + offset * 10, currentLevelPosition.Y);
            Vector2 missedEnemiesPosition = new Vector2(pointsPosition.X + playerPointsDimensions.X + offset * 10, currentLevelPosition.Y);

            int swarmTime = info.TimeToNextSwarm.Seconds;
            string nextSwarmText = "Next Swarm In " + swarmTime;
            float swarmScale = screenWidth / 900f;
            Vector2 middle = (font.MeasureString(nextSwarmText) * swarmScale) / 2;
            Vector2 nextSwarmTextPosition = new Vector2((screenWidth - middle.X) / 2 - middle.X / 2, (screenHeight - middle.Y) / 2 - -middle.Y / 2);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            #region Draw Current Level
            //draw the shadow
            DrawText(currentLevelText, currentLevelPosition + shadowOffset, fontScale, Color.Black);

            //draw text
            DrawText(currentLevelText, currentLevelPosition, fontScale, textColor);
            #endregion

            #region Draw Player Points
            //draw the shadow
            DrawText(playerPointsText, pointsPosition + shadowOffset, fontScale, Color.Black);

            //draw text
            DrawText(playerPointsText, pointsPosition, fontScale, textColor);
            #endregion

            #region Draw Spider Count
            //draw the shadow
            DrawText(spiderCountText, enemyCountPosition + shadowOffset, fontScale, Color.Black);

            //draw text
            DrawText(spiderCountText, enemyCountPosition, fontScale, textColor);

            #endregion

            #region Draw Missed Spider Count
            //draw the shadow
            DrawText(missedEnemiesCount, missedEnemiesPosition + shadowOffset, fontScale, Color.Black);

            //draw text
            DrawText(missedEnemiesCount, missedEnemiesPosition, fontScale, textColor);
            #endregion

            #region Draw Next Swarm Counter
            if (swarmTime != 0)
            {
                //draw the shadow
                DrawText(nextSwarmText, nextSwarmTextPosition + shadowOffset, swarmScale, Color.Black);

                //draw text
                DrawText(nextSwarmText, nextSwarmTextPosition, swarmScale, Color.Red);
            }
            #endregion

            spriteBatch.End();
        }

        public void DrawPausedScreen(string text)
        {
            int screenWidth = graphics.PreferredBackBufferWidth;
            int screenHeight = graphics.PreferredBackBufferHeight;
            float scale = .7f;
            Vector2 middle = (font.MeasureString(text) * scale) / 2;
            Vector2 position = new Vector2(
                (screenWidth - middle.X) / 2 - middle.X / 2,
                (screenHeight - middle.Y) / 2 - -middle.Y / 2);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            //draw shadow
            DrawText(text, position + shadowOffset, scale, Color.Black);

            //draw text
            DrawText(text, position, scale, Color.White);

            spriteBatch.End();
        }

        public void DrawInstructions(string[] text)
        {
            int screenWidth = graphics.PreferredBackBufferWidth;
            int screenHeight = graphics.PreferredBackBufferHeight;
            Vector2 textPosition = Vector2.Zero;
            float textScale = 1f;
            float textOffset = 40;

            string longestString = "";

            foreach (string str in text)
                if (longestString.Length < str.Length)
                    longestString = str;

            Vector2 dimensions = Vector2.Zero;

            //get the dimensions of the longest string
            foreach (string str in text)
                if (dimensions.X < font.MeasureString(str).X)
                    dimensions = font.MeasureString(str);

            //calculate the text dimensions based on the screen width and the length of the longest string
            textScale = screenWidth / (dimensions.X + textOffset);

            float spaceBetweenEntries = (screenHeight / (text.Length + 1f));

            Rectangle dimmedRectangle = new Rectangle(0, 0,
                screenWidth,
                screenHeight);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            spriteBatch.Draw(texture, dimmedRectangle, new Color(0, 0, 0, 127));

            for (int i = 0; i < text.Length; i++)
            {
                Vector2 textDimensions = (font.MeasureString(text[i]) * textScale);

                textPosition = new Vector2(
                (screenWidth - textDimensions.X) / 2,
                textPosition.Y + spaceBetweenEntries);

                //draw the shadow
                DrawText(text[i], textPosition + shadowOffset, textScale, Color.Black);

                //draw text
                DrawText(text[i], textPosition, textScale, Color.Yellow);
            }

            spriteBatch.End();
        }
    }
}
