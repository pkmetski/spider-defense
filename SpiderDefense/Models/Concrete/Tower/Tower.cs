using System.Collections.Generic;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class Tower : Building
    {
        private ContentManager content;
        private List<Arrow> arrows = new List<Arrow>();
        private TowerCharacteristics characteristics = new TowerCharacteristics();
        private TowerCharacteristics afterUpgr;
        private Timer shootingTimer = new Timer();
        private bool canShoot = true;
        private int arrowsCount = 10;
        private int id;

        public Tower(ContentManager content, Vector3 position, int id)
            : base(content.Load<Model>(@"Models\Tower"), position)
        {
            this.content = content;
            this.id = id;
            Init();
        }

        private void Init()
        {
            Selectable = true;
            Scale = Matrix.CreateScale(0.3f);
            SelectionCircleRadiusVariable = 3.5f;
            shootingTimer.Elapsed += new ElapsedEventHandler(shootingTimer_Elapsed);
            shootingTimer.AutoReset = true;
            shootingTimer.Interval = characteristics.ShootFrequency;
            CreateArrows();
        }

        private void shootingTimer_Elapsed(object obj, ElapsedEventArgs args)
        {
            CanShoot = true;
        }

        #region Properties
        public List<Arrow> Arrows
        {
            get { return arrows; }
            set { arrows = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public TowerCharacteristics Characteristics
        {
            get { return characteristics; }
            set { characteristics = value; }
        }

        public bool CanShoot
        {
            get { return canShoot; }
            set { canShoot = value; }
        }

        #endregion

        public override string[] OnScreenInfo
        {
            get
            {
                string[] onScreenInfo = new string[6];
                onScreenInfo[0] = "Tower " + Id;
                onScreenInfo[1] = "Current Level: " + characteristics.CurrentLevel;
                onScreenInfo[2] = "Range: " + characteristics.Range;
                onScreenInfo[3] = "Damage: " + characteristics.Damage;
                onScreenInfo[4] = "Arrow Speed: " + characteristics.ArrowSpeed;
                onScreenInfo[5] = "Shooting Frequency(seconds): " + characteristics.ShootFrequency / 1000;
                return onScreenInfo;
            }
        }

        private void CreateArrows()
        {
            for (int i = 0; i < arrowsCount; i++)
            {
                Arrow arrow = new Arrow(content, new Vector3(Position.X, Position.Y + 20, Position.Z));
                arrow.Attack = characteristics.Damage;
                arrow.Speed = characteristics.ArrowSpeed;
                arrows.Add(arrow);
            }
        }

        public void Shoot(Vector3 target)
        {
            if (Vector3.Distance(Position, target) < characteristics.Range && CanShoot)
            {
                //shot with the first available arrow
                foreach (Arrow arrow in arrows)
                    if (!arrow.IsMoving)
                    {
                        arrow.SetMoving(target);
                        if (CanShoot)
                        {
                            CanShoot = false;
                            shootingTimer.Start();
                        }
                        break;
                    }
            }
        }

        private void UpgradeArrows()
        {
            foreach (Arrow arrow in arrows)
            {
                arrow.Attack = characteristics.Damage;
                arrow.Speed = characteristics.ArrowSpeed;
            }
        }

        public void Upgrade(int totalPoints)
        {
            if (totalPoints >= characteristics.UpgradeFee)
            {
                characteristics.Upgrade();
                shootingTimer.Interval = characteristics.ShootFrequency;
                afterUpgr = null;
                UpgradeArrows();
            }
        }

        public TowerCharacteristics AfterUpgrade()
        {
            if (afterUpgr == null)
            {
                afterUpgr = (TowerCharacteristics)characteristics.Clone();
                afterUpgr.Upgrade();
            }
            return afterUpgr;
        }
    }
}
