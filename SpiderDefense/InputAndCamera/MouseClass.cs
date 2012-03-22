using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpiderDefense
{
    class MouseClass
    {
        SpriteBatch spriteBatch;

        Rectangle mSelectionBox;
        Texture2D selectionLine;
        Color selectionColor = Color.Black;

        MouseState currentState;
        MouseState originalPositionState;//used for retrieving the original cursor position (before centering)
        MouseState previousMouseState;

        Vector2 position = Vector2.Zero;

        bool firstPass = true;//indicates that the previous mouse state should be saved in the first state variable
        bool secondPass = false;
        bool cursorNeedsRepositioning = false;
        bool showCursor = true;

        public MouseClass(SpriteBatch spriteBatch, Texture2D selectionLine)
        {
            this.spriteBatch = spriteBatch;
            this.selectionLine = selectionLine;
            mSelectionBox = new Rectangle(0, 0, 0, 0);
        }

        #region Properties
        public MouseState CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }

        public MouseState OriginalPositionState
        {
            get { return originalPositionState; }
            set { originalPositionState = value; }
        }

        public MouseState PreviousState
        {
            get { return previousMouseState; }
            set { previousMouseState = value; }
        }

        public Vector2 Position
        {
            get { return new Vector2(currentState.X, currentState.Y); }
        }

        public bool FirstPass
        {
            get { return firstPass; }
            set { firstPass = value; }
        }

        public bool SecondPass
        {
            get { return secondPass; }
            set { secondPass = value; }
        }

        public bool CursorNeedsRepositioning
        {
            get { return cursorNeedsRepositioning; }
            set { cursorNeedsRepositioning = value; }
        }

        public bool ShowCursor
        {
            get { return showCursor; }
            set { showCursor = value; }
        }

        public Rectangle SelectionRectangle
        {
            get { return mSelectionBox; }
            set { mSelectionBox = value; }
        }

        //returns a rectangle always with positive height and width 
        //and position on the upper left corner
        public Rectangle AbsoluteSelRectangle
        {
            get
            {
                Rectangle rec = mSelectionBox;
                if (rec.Width < 0)
                {
                    rec.X += rec.Width;
                    rec.Width = -rec.Width;
                }

                if (rec.Height < 0)
                {
                    rec.Y += rec.Height;
                    rec.Height = -rec.Height;
                } 
                return rec; }
        }

        #endregion

        public void DrawHorizontalLine(int thePositionY)
        {
            //When the width is greater than 0, the user is selecting an area to the right of the starting point
            if (mSelectionBox.Width > 0)
            {
                //Draw the line starting at the startring location and moving to the right
                for (int aCounter = 0; aCounter <= mSelectionBox.Width; aCounter++)
                {
                    if (mSelectionBox.Width - aCounter >= 0 && currentState.LeftButton==ButtonState.Pressed)
                    {
                        spriteBatch.Draw(selectionLine, new Rectangle(mSelectionBox.X + aCounter - 1, thePositionY - 1, 1, 1), selectionColor);
                    }
                }
            }
            //When the width is less than 0, the user is selecting an area to the left of the starting point
            else if (mSelectionBox.Width < 0)
            {
                //Draw the line starting at the starting location and moving to the left
                for (int aCounter = 0; aCounter >= mSelectionBox.Width; aCounter--)
                {
                    if (mSelectionBox.Width - aCounter <= 0 && currentState.LeftButton == ButtonState.Pressed)
                    {
                        spriteBatch.Draw(selectionLine, new Rectangle(mSelectionBox.X + aCounter - 1, thePositionY - 1, 1, 1), selectionColor);
                    }
                }
            }
        }

        public void DrawVerticalLine(int thePositionX)
        {
            //When the height is greater than 0, the user is selecting an area below the starting point
            if (mSelectionBox.Height > 0)
            {
                //Draw the line starting at the starting location and moving down
                for (int aCounter = 0; aCounter <= mSelectionBox.Height; aCounter++)
                {
                    if (mSelectionBox.Height - aCounter >= 0 && currentState.LeftButton == ButtonState.Pressed)
                    {
                        spriteBatch.Draw(selectionLine, new Rectangle(thePositionX, mSelectionBox.Y + aCounter - 1, 1, 1), new Rectangle(0, 0, selectionLine.Width, selectionLine.Height), selectionColor, MathHelper.ToRadians(90), new Vector2(0, 0), SpriteEffects.None, 0);
                    }
                }
            }
            //When the height is less than 0, the user is selecting an area above the starting point
            else if (mSelectionBox.Height < 0)
            {
                //Draw the line starting at the start location and moving up
                for (int aCounter = 0; aCounter >= mSelectionBox.Height; aCounter--)
                {
                    if (mSelectionBox.Height - aCounter <= 0 && currentState.LeftButton == ButtonState.Pressed)
                    {
                        spriteBatch.Draw(selectionLine, new Rectangle(thePositionX - 1, mSelectionBox.Y + aCounter - 1, 1, 1), selectionColor);
                    }
                }
            }
        }

        public Ray CalculateCursorRay(Matrix projectionMatrix, Matrix viewMatrix, GraphicsDeviceManager graphics)
        {
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3(Position, 0f);
            Vector3 farSource = new Vector3(Position, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }
    }
}