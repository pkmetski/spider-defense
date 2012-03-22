using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class Spider : AnimatedModel
    {
        private ContentManager content;
        private int level;
        private string[] onScreenInfo = new string[7];

        public Spider(ContentManager content, MapPath path, Camera camera)
            : base(content.Load<Model>(@"Models\EnemyBeast"), path, camera)
        {
            this.content = content;
            Init();
        }

        public Spider(ContentManager content, Vector3 position, Camera camera)
            : base(content.Load<Model>(@"Models\EnemyBeast"), position, camera)
        {
            this.content = content;
            Init();
        }

        private void Init()
        {
            Selectable = true;
            SelectionCircleRadiusVariable = 1.5f;
            Scale = Matrix.CreateScale(0.4f);
            TopInfoOffset = new Vector3(0.0f, 6.5f, 0.0f);
            HealthBarOffset = new Vector2(0, -6);
            PointsPerKill = 1;

            // Animation
            ActiveAnimation = Animations[0];
            ActiveAnimationTime = TimeSpan.Zero;
        }

        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        public override string[] OnScreenInfo
        {
            get
            {
                onScreenInfo[0] = "Level " + level + " Spider";
                onScreenInfo[1] = "Health Left: " + CurrentHP.ToString("#0.0");
                onScreenInfo[2] = "Total Health: " + TotalHP.ToString("#0.0");
                onScreenInfo[3] = "Remaining Health(%): " + ((CurrentHP / TotalHP) * 100).ToString("#0.00");
                onScreenInfo[4] = "Shield(%): " + (Shield * 100).ToString("#0.00");
                onScreenInfo[5] = "Speed: " + (Speed * 100).ToString("#0.00");
                if (DistanceToDestination != -1)
                    onScreenInfo[6] = "Distance To Destination: " + DistanceToDestination.ToString("#0.0");
                else
                    onScreenInfo[6] = "Distance To Destination: N/A";
                return onScreenInfo;
            }
        }

        public override string ModelTopInfo
        {
            get { return ""; }
        }

        public override void MoveUnit()
        {
            base.MoveUnit();

            //adjust model's position according to the terrain height
            Position = new Vector3(Position.X,
                SelectionCircleAttributes.HeightValues[(int)Position.X, -(int)Position.Z],
                Position.Z);
        }

        public override bool IsMoving
        {
            get { return base.IsMoving; }
            set
            {
                if (!value)
                {
                    ActiveAnimation = Animations[0];
                }
                else
                {
                    ActiveAnimation = Animations[1];
                }

                base.IsMoving = value;
            }
        }

        public override bool IsDying
        {
            get { return base.IsDying; }
            set
            {
                if (value && !IsDying)
                {
                    ActiveAnimation = Animations[4];
                    EnableAnimationLoop = false;
                }
                base.IsDying = value;
            }
        }

        public override AnimatedModelContent.AnimationData ActiveAnimation
        {
            get { return base.ActiveAnimation; }
            set
            {
                if (value == Animations[1])
                    AnimationSpeed = Speed * 8f;
                else
                    AnimationSpeed = 1.0f;
                base.ActiveAnimation = value;
            }
        }
    }
}
