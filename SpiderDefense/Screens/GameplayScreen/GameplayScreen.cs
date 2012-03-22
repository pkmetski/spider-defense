using System;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpiderDefense
{
    public class GameplayScreen : GameScreen
    {
        #region Fields

        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        ContentManager content;
        SpriteBatch spriteBatch;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;

        Camera camera;
        Effect advancedEffect;
        BasicEffect basicEffect;
        Matrix worldMatrix = Matrix.Identity;
        MouseClass myMouse;
        KeyboardState keyboardState;
        KeyboardState previousKeyboardState;
        Map map;
        Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.3f);

        RightClickFlag rightClickFlag;
        OnScreenInfo onScreenInfo;
        LevelInfo lvlInfo;
        SpiderDispatcher dispatcher;

        bool gamePaused = false;
        bool menuActive = false;
        bool firstStart = true;
        string[] instructions = new string[] { 
            "The spiders will move on the path trying to reach the other end of the map.", 
            "Your goal is to stop them using the towers.", 
            "Each killed spider grants you some points.", 
            "After each level the spiders will become faster and stronger.", 
            "You will have to upgrade your towers in order to keep up.", 
            "To upgrade a tower you use the points you have accumulated.", 
            "", 
            "Press Any Key To Continue..." };
        AutoResetEvent pauseEvent;

        #endregion

        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            graphics = ScreenManager.GraphicsDeviceManager;
            content = new ContentManager(ScreenManager.Game.Services, "Content");
            spriteBatch = ScreenManager.SpriteBatch;
            device = graphics.GraphicsDevice;

            advancedEffect = content.Load<Effect>(@"Effects\Series4Effects");
            basicEffect = new BasicEffect(device);
            pauseEvent = new AutoResetEvent(true);
            rightClickFlag = new RightClickFlag(content);
            SetupMouse();
            SetupMap();
            SetupCamera();

            dispatcher = new SpiderDispatcher(content, map, camera, pauseEvent);
            onScreenInfo = new OnScreenInfo(graphics, camera, spriteBatch, content);
            lvlInfo = new LevelInfo();

            base.LoadContent();
        }

        private void SetupCamera()
        {
            float maxHeight = map.HighestPoint;
            camera = new Camera(new Vector3(70.0f, 25 + maxHeight, 30.0f),
                new Vector3(60.0f, 0.0f, -70.0f),
                graphics);
        }

        private void SetupMouse()
        {
            myMouse = new MouseClass(spriteBatch, content.Load<Texture2D>(@"Textures\blank"));
            ((GameStateManagementGame)ScreenManager.Game).IsMouseVisible = myMouse.ShowCursor;
        }

        private void SetupMap()
        {
            map = new Map(content, 10.0f, 40.0f);

            InitializeBuffers();
        }

        private void InitializeBuffers()
        {
            vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), map.TerrainVertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(map.TerrainVertices);
            indexBuffer = new IndexBuffer(device, typeof(int), map.Indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(map.Indices);
        }

        private void StartGame()
        {
            dispatcher.SpiderCount = 6;
            Thread spiderDispatchThread = new Thread(dispatcher.DispatchSpider);
            spiderDispatchThread.IsBackground = true;
            //spiderDispatchThread.Start();
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            menuActive = false;
            myMouse.ShowCursor = true;
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];

            if (input.IsPauseGame(ControllingPlayer))
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            if (ScreenManager.GetScreens().OfType<PauseMenuScreen>().Count() > 0)
            {
                myMouse.ShowCursor = false;
                menuActive = true;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                      bool coveredByOtherScreen)
        {
            keyboardState = Keyboard.GetState();
            myMouse.CurrentState = Mouse.GetState();

            #region Dismiss Instructions Screen And Start Game

            if (firstStart && keyboardState.GetPressedKeys().Length > 0)
            {
                StartGame();
                firstStart = false;
            }

            #endregion

            #region Exit Game Over With Any Key
            if (lvlInfo.GameOver &&
                keyboardState.GetPressedKeys().Length > 0)
                LoadingScreen.Load(ScreenManager, false, null,
                    new BackgroundScreen(),
                    new MainMenuScreen());
            #endregion

            #region Pause Game

            if ((keyboardState.IsKeyDown(Keys.P) &&
                !previousKeyboardState.IsKeyDown(Keys.P)) ||
                keyboardState.IsKeyDown(Keys.MediaPlayPause) &&
                !previousKeyboardState.IsKeyDown(Keys.MediaPlayPause))
                gamePaused = !gamePaused;

            #endregion

            if (!gamePaused &&
                !menuActive &&
                !lvlInfo.GameOver)
            {
                pauseEvent.Set();

                #region Selection Rectangle Creation
                if (myMouse.CurrentState.LeftButton == ButtonState.Pressed && myMouse.PreviousState.LeftButton == ButtonState.Released)
                    myMouse.SelectionRectangle = new Rectangle(myMouse.CurrentState.X, myMouse.CurrentState.Y, 0, 0);

                if (myMouse.CurrentState.LeftButton == ButtonState.Pressed)
                    myMouse.SelectionRectangle = new Rectangle(myMouse.SelectionRectangle.X, myMouse.SelectionRectangle.Y, myMouse.CurrentState.X - myMouse.SelectionRectangle.X, myMouse.CurrentState.Y - myMouse.SelectionRectangle.Y);
                #endregion

                #region Upgrade Tower
                if (map.SelectedModels.Count == 1 &&
                    map.SelectedModels[0] is Tower &&
                    myMouse.CurrentState.LeftButton == ButtonState.Pressed &&
                    myMouse.PreviousState.LeftButton == ButtonState.Released &&
                    onScreenInfo.upgradeButton.Intersects(new Rectangle((int)myMouse.Position.X, (int)myMouse.Position.Y, 1, 1)))
                {
                    ((Tower)map.SelectedModels[0]).Upgrade(lvlInfo.PlayerPoints);
                    lvlInfo.DeductPoints(((Tower)map.SelectedModels[0]).Characteristics.UpgradeFee);
                }
                #endregion

                #region Selecting Unit
                //select a model when the player left clicks on it
                if (myMouse.CurrentState.LeftButton == ButtonState.Pressed &&
                    myMouse.PreviousState.LeftButton == ButtonState.Released)
                    foreach (MapModel mapModel in map.AllMapModels)
                    {
                        if (mapModel.ToBeSelected(myMouse, camera, graphics))
                        {
                            if (!map.SelectedModels.Contains(mapModel))
                            {
                                mapModel.Selected = true;
                                map.SelectedModels.Add(mapModel);
                            }
                        }
                        else
                        {
                            if (!(mapModel is Tower) ||
                                !onScreenInfo.upgradeButton.Intersects(new Rectangle((int)myMouse.Position.X, (int)myMouse.Position.Y, 1, 1)))
                            {
                                mapModel.Selected = false;
                                map.SelectedModels.Remove(mapModel);
                            }
                        }
                    }

                //select a unit with the mouse's selection rectangle
                if (myMouse.CurrentState.LeftButton == ButtonState.Pressed &&
                    myMouse.SelectionRectangle.Width != 0 &&
                    myMouse.SelectionRectangle.Height != 0)
                    foreach (MapModel mapModel in map.AllMapModels)
                        if (mapModel is DynamicModel)
                            if (((DynamicModel)mapModel).Intersects(myMouse.AbsoluteSelRectangle) && mapModel.Selectable)
                            {
                                if (!map.SelectedModels.Contains(mapModel))
                                {
                                    ((DynamicModel)mapModel).Selected = true;
                                    map.SelectedModels.Add(mapModel);
                                }
                            }
                #endregion

                #region Handling Right Click
                if (myMouse.CurrentState.RightButton == ButtonState.Pressed &&
                    myMouse.PreviousState.RightButton != ButtonState.Pressed)
                {
                    rightClickFlag.Position = map.TerrainCollision(myMouse.CalculateCursorRay(camera.Projection,
                                                                                            camera.View,
                                                                                            graphics));
                    foreach (MapModel mapModel in map.SelectedModels)
                    {
                        //moving a dynamic model
                        if (mapModel is DynamicModel && !(mapModel is Arrow))
                            if (((DynamicModel)mapModel).Belongs && !((DynamicModel)mapModel).IsDying)//if the unit belongs to the player, then move it
                                ((DynamicModel)mapModel).SetMoving(rightClickFlag.Position);
                        //shooting an arrow
                        if (mapModel is Tower)
                            ((Tower)mapModel).Shoot(rightClickFlag.Position);
                    }
                    rightClickFlag.Reset();
                }
                if (rightClickFlag.Draw)
                    rightClickFlag.TickTimer();
                #endregion

                #region Move, Animate Units and Handle Collision
                for (int i = 0; i < map.AllMapModels.Count; i++)
                {
                    MapModel mapModel = map.AllMapModels[i];
                    if (mapModel is DynamicModel)
                    {
                        //move unit
                        if (((DynamicModel)mapModel).IsMoving &&
                            ((DynamicModel)mapModel).IsAlive &&
                            !((DynamicModel)mapModel).IsDying)
                        {
                            DynamicModel unit = (DynamicModel)mapModel;
                            unit.PreviousPosition = unit.Position;
                            unit.MoveUnit();
                        }
                        if (((DynamicModel)mapModel).IsDying)
                            ((DynamicModel)mapModel).ToDieTimer += 10;

                        //update missed spider count
                        if (((DynamicModel)mapModel).ReachedEndOfPath)
                        {
                            ((DynamicModel)mapModel).ReachedEndOfPath = false;
                            lvlInfo.MissedEnemies++;
                        }

                        //animate unit
                        if (mapModel is AnimatedModel)
                        {
                            ((AnimatedModel)mapModel).Update(gameTime);
                        }

                        //handle collision
                        if (mapModel is Arrow)
                        {
                            Arrow arrow = (Arrow)mapModel;
                            foreach (MapModel mapModel2 in map.AllMapModels)
                            {
                                if (!(mapModel2 is Arrow) &&
                                    !(mapModel2 is Tower) &&
                                    mapModel2 is DynamicModel)
                                {
                                    if (!((DynamicModel)mapModel).IsDying)
                                    {
                                        if (mapModel2.IntersectsWithModel(arrow))
                                        {
                                            if (((DynamicModel)mapModel2).InflictDamage(arrow.Attack))
                                                lvlInfo.PlayerPoints += ((DynamicModel)mapModel2).PointsPerKill;
                                            arrow.IsMoving = false;
                                            arrow.Position = arrow.OriginalPosition;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Towers Automatic Shoot

                foreach (Tower tower in map.Towers)
                {
                    Vector3 target = Vector3.Zero;
                    float distance = float.MaxValue;

                    foreach (MapModel mapModel in map.AllMapModels)
                    {
                        if (mapModel is Spider)
                        {
                            Spider spider = (Spider)mapModel;
                            if (!spider.IsDying)
                            {
                                float tempDistance = Vector3.Distance(tower.Position, spider.Position);

                                if (distance > tempDistance)
                                {
                                    distance = tempDistance;
                                    target = spider.Position;
                                }
                            }
                        }
                    }
                    if (target != Vector3.Zero)
                        tower.Shoot(target);
                }

                #endregion

                #region Update Level Info

                lvlInfo.EnemyCount = map.AllMapModels.Count(n => n is Spider);
                lvlInfo.CurrentGameLevel = dispatcher.Level;
                lvlInfo.TimeToNextSwarm = dispatcher.TimeToNextSwarm;

                #endregion
            }

            #region Calculate Model-Camera Distance
            //calculate the distance between the camera and each model
            foreach (MapModel mapModel in map.AllMapModels)
                mapModel.DistanceFromCamera = Vector3.Distance(mapModel.Position, camera.Position);
            #endregion

            if (!menuActive)
            {
                camera.ProcessInput(keyboardState, myMouse, map.HeightValues);
            }
            myMouse.PreviousState = myMouse.CurrentState;
            previousKeyboardState = keyboardState;
            ((GameStateManagementGame)ScreenManager.Game).IsMouseVisible = myMouse.ShowCursor;

            //remove the "dead" models from the map
            map.RemoveDead();

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }



        public override void Draw(GameTime gameTime)
        {
            device.Clear(Color.Gray);
            //device.RenderState.FillMode = FillMode.WireFrame;

            #region Draw Terrain

            DrawTerrain();

            #endregion

            #region Draw Selection Circles
            foreach (MapModel mapModel in map.SelectedModels)
                if (mapModel.DrawSelectionCircle)
                    DrawSelectionCircle(mapModel);
            #endregion

            #region Draw Path
            foreach (MapModel pathSegment in map.PathModels)
                DrawModel(pathSegment, gameTime);
            #endregion

            #region Draw Fence

            foreach (MapModel fence in map.FenceModels)
                DrawModel(fence, gameTime);

            #endregion

            #region Draw Models
            foreach (MapModel mapModel in map.AllMapModels)
            {
                bool draw = true;
                if (mapModel is DynamicModel)
                    if (!((DynamicModel)mapModel).IsAlive)
                        draw = false;
                if (mapModel is Arrow)
                    if (!((Arrow)mapModel).IsMoving)
                        draw = false;
                if (draw)
                    DrawModel(mapModel, gameTime);
            }
            #endregion

            #region Draw HealthBar
            foreach (MapModel mapModel in map.SelectedModels)
                if (mapModel is AnimatedModel)
                {
                    onScreenInfo.DrawHealthBar((Spider)mapModel);
                }
            #endregion

            #region Draw Right Click Flag

            if (rightClickFlag.Draw && rightClickFlag.Position != Vector3.Zero)
                DrawModel(rightClickFlag, gameTime);

            #endregion

            #region Draw Selection Rectangle
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            //Draw the horizontal portions of the selection box 
            myMouse.DrawHorizontalLine(myMouse.SelectionRectangle.Y);
            myMouse.DrawHorizontalLine(myMouse.SelectionRectangle.Y + myMouse.SelectionRectangle.Height);

            //Draw the vertical portions of the selection box 
            myMouse.DrawVerticalLine(myMouse.SelectionRectangle.X);
            myMouse.DrawVerticalLine(myMouse.SelectionRectangle.X + myMouse.SelectionRectangle.Width);
            spriteBatch.End();
            #endregion

            #region Draw On Screen Info


            for (int i = 0; i < map.AllMapModels.Count; i++)
            {
                MapModel mapModel = map.AllMapModels[i];
                if (mapModel.TwoDProjection.X > 0 &&
                   mapModel.TwoDProjection.X < graphics.PreferredBackBufferWidth &&
                   mapModel.TwoDProjection.Y > 0 &&
                   mapModel.TwoDProjection.Y - 60 < graphics.PreferredBackBufferHeight)
                {

                    if (!mapModel.ModelTopInfo.Equals(""))
                        onScreenInfo.DrawModelTopInfo(mapModel);
                }
                if (map.SelectedModels.Count == 1 &&
                    !map.SelectedModels[0].OnScreenInfo.Equals(""))
                    onScreenInfo.DrawInfo(map.SelectedModels[0].OnScreenInfo);

                if (map.SelectedModels.Count == 1 &&
                    mapModel is Tower && mapModel.Selected)
                    onScreenInfo.DrawUpgradeRectangle((Tower)mapModel);
            }

            onScreenInfo.DrawLevelInfo(lvlInfo);

            if (gamePaused)
                onScreenInfo.DrawPausedScreen("Paused");
            if (lvlInfo.GameOver)
                onScreenInfo.DrawPausedScreen("Game Over");
            if (firstStart)
                onScreenInfo.DrawInstructions(instructions);
            #endregion

            base.Draw(gameTime);
        }

        private void DrawTerrain()
        {
            lightDirection.Normalize();

            advancedEffect.CurrentTechnique = advancedEffect.Techniques["Textured"];
            advancedEffect.Parameters["xTexture"].SetValue(map.Texture);
            advancedEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            advancedEffect.Parameters["xView"].SetValue(camera.View);
            advancedEffect.Parameters["xProjection"].SetValue(camera.Projection);
            advancedEffect.Parameters["xEnableLighting"].SetValue(true);
            advancedEffect.Parameters["xLightDirection"].SetValue(lightDirection);
            advancedEffect.Parameters["xAmbient"].SetValue(0.2f);

            foreach (EffectPass pass in advancedEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.Indices = indexBuffer;
                device.SetVertexBuffer(vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, map.TerrainVertices.Length, 0, map.Indices.Length / 3);

            }
        }

        private void DrawModel(MapModel mapModel, GameTime gameTime)
        {
            Matrix[] transforms;
            //if the model passed is a unit, it means it is  animated,
            //therefore the bonetransofrms needs to be recalculated every time
            if (mapModel.AbsoluteBoneTransforms == null)
            {
                transforms = new Matrix[mapModel.Model.Bones.Count];
                mapModel.Model.CopyAbsoluteBoneTransformsTo(transforms);
                mapModel.AbsoluteBoneTransforms = transforms;
            }
            else
                transforms = mapModel.AbsoluteBoneTransforms;

            mapModel.TwoDProjection = graphics.GraphicsDevice.Viewport.Project(mapModel.Position,
                     camera.Projection,
                     camera.View,
                     Matrix.Identity);

            if (mapModel is AnimatedModel)
            {
                ((AnimatedModel)mapModel).Draw(gameTime);
            }
            else
            {
                foreach (ModelMesh mesh in mapModel.Model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;

                        effect.Projection = camera.Projection;
                        effect.View = camera.View;
                        effect.World = transforms[mesh.ParentBone.Index] * mapModel.World;
                    }
                    mesh.Draw();
                }
            }
        }

        private void DrawTower(Model model, Vector3 position)
        {
            //graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            // TODO: Add your drawing code here
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {

                    effect.Projection = camera.Projection;
                    effect.View = Matrix.CreateLookAt(camera.Position, camera.Target, Vector3.Up);
                    effect.World = transforms[mesh.ParentBone.Index] *
                                     Matrix.CreateScale(0.3f) * Matrix.CreateTranslation(position);
                }
                mesh.Draw();
            }
        }

        private void DrawSelectionCircle(MapModel mapModel)
        {
            if (mapModel is AnimatedModel)
                ((AnimatedModel)mapModel).UpdateSelectionCircle();
            if (mapModel.CirclePoints != null)
            {
                for (int i = 0; i < mapModel.CirclePoints.Length; i++)
                {
                    device.DrawUserIndexedPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        mapModel.CirclePoints[i],
                        0,   // vertex buffer offset to add to each element of the index buffer
                        mapModel.CirclePoints[i].Length,   // number of vertices to draw
                        mapModel.CircleIndices[i],
                        0,   // first index element to read
                        mapModel.CirclePoints[i].Length    // number of primitives to draw
                        );
                }
            }
        }
    }
}