using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class Arrow : DynamicModel
    {
        private double timer;
        private const float visibleTime = 1000.0f;
        private Vector3 originalPosition;

        public Arrow(ContentManager content, Vector3 position)
            : base(content.Load<Model>(@"Models\WoodenArrow"), position)
        {
            originalPosition = position;
            Initialize();
        }

        private void Initialize()
        {
            Scale = Matrix.CreateScale(0.1f);
            Selectable = false;
            DrawSelectionCircle = false;
            Belongs = false;
        }

        public Vector3 OriginalPosition
        {
            get { return originalPosition; }
        }

        public override string[] OnScreenInfo
        {
            get
            {
                string[] onScreenInfo = new string[3];
                onScreenInfo[0] = "Arrow";
                onScreenInfo[1] = "Attack: " + Attack;
                onScreenInfo[2] = "Speed: " + Speed;
                return onScreenInfo;
            }
        }

        public override string ModelTopInfo
        {
            get { return ""; }
        }

        public override void SetMoving(Vector3 destination)
        {
            IsMoving = true;
            Rotation = CalculateAngle(destination) * base.RotateToFace(destination);
            Destination = destination;
        }

        private Matrix CalculateAngle(Vector3 destination)
        {
            float distance = Vector3.Distance(Position, destination);
            distance = MathHelper.Clamp(distance, 1, float.MaxValue);
            float sin = Position.Y / (distance * 2);
            float angle = (float)Math.Asin(sin);
            angle = MathHelper.Clamp(angle, 0, 80);
            return Matrix.CreateRotationX(angle);
        }

        public override void MoveUnit()
        {
            if (Vector3.Distance(Position, Destination) > 1)
                base.MoveUnit();
            else
                timer += 10;

            if (timer >= visibleTime)
            {
                IsMoving = false;
                Position = originalPosition;
                timer = 0;
            }
        }
    }
}
