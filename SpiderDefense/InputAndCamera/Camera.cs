using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SpiderDefense
{
    class Camera
    {
        private Matrix viewMatrix;
        private Matrix projectionMatrix;
        private Matrix rotateMatrix = Matrix.Identity;

        private Vector3 cameraPosition;
        private Vector3 cameraTarget;
        private Vector3 cameraUp = Vector3.Up;
        private GraphicsDeviceManager graphics;
        private float aspectRatio;
        private int width;
        private int height;
        private bool isFullScreen;
        private float accumulatedScale = 1;

        private const float minScale = 0.5f;
        private const float maxScale = 1.2f;
        private float minHeight = 15.0f;
        private float maxHeight = 50.0f;

        private const float cameraMoveStep = 0.9f;
        private const float cameraLiftStep = 0.9f;
        private const float cameraRotationStep = 3.0f;
        private const float cameraScaleStep = 0.1f;
        private const float liftSensitivity = 3.0f;
        private const float rotationSensitivity = 5.0f;
        private const float middleClickMovementSensitivity = 5.0f;

        public Camera(Vector3 cameraPosition, Vector3 cameraLookAt, GraphicsDeviceManager graphics)
        {
            this.cameraPosition = cameraPosition;
            this.cameraTarget = cameraLookAt;
            this.graphics = graphics;
            InitializeCamera();
        }

        public Matrix Projection
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        }

        public Matrix View
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        public Vector3 Position
        {
            get { return cameraPosition; }
        }

        public Vector3 Target
        {
            get { return cameraTarget; }
        }

        public void InitializeCamera()
        {
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUp);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.2f, 500.0f);
            this.aspectRatio = (float)graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;
            this.width = graphics.PreferredBackBufferWidth;
            this.height = graphics.PreferredBackBufferHeight;
            this.isFullScreen = graphics.IsFullScreen;
        }

        public void ProcessInput(KeyboardState keyboardState, MouseClass myMouse, float[,] heightValues)
        {
            float rotationAngle = 0;
            float currentScale = 1.0f;
            Vector3 moveVector = Vector3.Zero;

            //rotate the camera around camera target
            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.Right))
            {
                rotationAngle += cameraRotationStep;
            }
            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.Left))
            {
                rotationAngle -= cameraRotationStep;
            }

            //move the camera on the X/Z axis
            if (((!keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.Left)) ||
                (myMouse.CurrentState.MiddleButton == ButtonState.Pressed && myMouse.CurrentState.X - myMouse.PreviousState.X < middleClickMovementSensitivity) ||
                (myMouse.CurrentState.X <= 0 && isFullScreen)) && myMouse.CurrentState.RightButton != ButtonState.Pressed)
            {
                moveVector.X -= cameraMoveStep;
            }
            if ((!keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.Right) ||
                (myMouse.CurrentState.MiddleButton == ButtonState.Pressed && myMouse.CurrentState.X - myMouse.PreviousState.X > -middleClickMovementSensitivity) ||
                (myMouse.CurrentState.X >= width - 1 && isFullScreen)) && myMouse.CurrentState.RightButton != ButtonState.Pressed)
            {
                moveVector.X += cameraMoveStep;
            }
            if ((!keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.Up) ||
                (myMouse.CurrentState.MiddleButton == ButtonState.Pressed && myMouse.CurrentState.Y - myMouse.PreviousState.Y < middleClickMovementSensitivity) ||
                (myMouse.CurrentState.Y <= 0 && isFullScreen)) && myMouse.CurrentState.RightButton != ButtonState.Pressed)
            {
                moveVector.Z -= cameraMoveStep;
            }
            if ((!keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.Down) ||
                (myMouse.CurrentState.MiddleButton == ButtonState.Pressed && myMouse.PreviousState.Y - myMouse.CurrentState.Y < middleClickMovementSensitivity) ||
                (myMouse.CurrentState.Y >= height - 1 && isFullScreen)) && myMouse.CurrentState.RightButton != ButtonState.Pressed)
            {
                moveVector.Z += cameraMoveStep;
            }

            //lift the camera up/down
            if (keyboardState.IsKeyDown(Keys.PageUp) ||
                (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.Up)))
            {
                cameraPosition.Y += cameraLiftStep;
            }
            if (keyboardState.IsKeyDown(Keys.PageDown) ||
                (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.Down)))
            {
                cameraPosition.Y -= cameraLiftStep;
            }

            //zoom in out
            if (accumulatedScale > minScale && myMouse.PreviousState.ScrollWheelValue < myMouse.CurrentState.ScrollWheelValue)
            {
                currentScale -= cameraScaleStep;
                accumulatedScale -= cameraScaleStep;
            }

            if (accumulatedScale < maxScale && myMouse.PreviousState.ScrollWheelValue > myMouse.CurrentState.ScrollWheelValue)
            {
                currentScale += cameraScaleStep;
                accumulatedScale += cameraScaleStep;
            }

            //scale scene
            Matrix scaleMatrix = Matrix.CreateScale(currentScale);

            //rotate camera
            Matrix currentRoation = Matrix.CreateRotationY(MathHelper.ToRadians(rotationAngle));
            rotateMatrix *= currentRoation;

            //move camera
            moveVector = Vector3.Transform(moveVector, rotateMatrix);
            Matrix translateMatrix = Matrix.CreateTranslation(moveVector);
            cameraTarget = Vector3.Transform(cameraTarget, translateMatrix);

            //combine the matrices
            Matrix tranformationMatrix = scaleMatrix * currentRoation * translateMatrix;

            //calculate the new camera position
            Vector3 reference = cameraPosition - cameraTarget;
            Vector3 transformedReference = Vector3.Transform(reference, tranformationMatrix);
            cameraPosition = transformedReference + cameraTarget;

            cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, minHeight, maxHeight);
            InitializeCamera();
        }
    }
}
