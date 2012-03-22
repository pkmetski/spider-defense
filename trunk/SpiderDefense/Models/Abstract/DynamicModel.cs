using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    abstract class DynamicModel : MapModel
    {
        private Vector3 destination;
        private float speed;
        //indicates whether the unit is currently moving
        private bool isMoving = false;
        private bool isAlive = true;
        private bool isDying = false;
        private bool reachedEndOfPath = false;
        private float totalHP;
        private float currentHP;
        //the shield decreases damage by a certain percent
        private float shield;
        private float attack;
        private float range;
        //determines which direction is considered "forward" for this unit
        private Vector3 forward = Vector3.Forward;

        private Vector3 previousPosition;

        private Vector2 healthBarOffset;
        //a flag indicating if the unit belongs to the player, or is an enemy unit
        private bool belongs;
        private float toDieTimer;

        //indicates how many points the player will receive after killing this creature
        private int pointsPerKill;

        private MapPath path;
        private PathCheckpoint lastCheckpoint;
        private PathCheckpoint nextCheckpoint;

        private float DIE_DELAY = 800;


        #region Properties
        public Vector3 MovingDirection
        {
            get { return Destination; }
            set { Destination = value; }
        }

        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public virtual bool IsMoving
        {
            get { return isMoving; }
            set { isMoving = value; }
        }

        public bool IsAlive
        {
            get { return isAlive; }
            set { isAlive = value; }
        }

        public virtual bool IsDying
        {
            get { return isDying; }
            set { isDying = value; }
        }

        public bool ReachedEndOfPath
        {
            get { return reachedEndOfPath; }
            set { reachedEndOfPath = value; }
        }

        public float TotalHP
        {
            get { return totalHP; }
            set { totalHP = value; }
        }

        public float CurrentHP
        {
            get { return currentHP; }
            set
            {
                currentHP = value;
                currentHP = MathHelper.Clamp(currentHP, 0, totalHP);
            }
        }

        public float Shield
        {
            get { return shield; }
            set
            {
                shield = value;
                shield = MathHelper.Clamp(shield, 0.00001f, 1);
            }
        }

        public float Attack
        {
            get { return attack; }
            set { attack = value; }
        }

        public float Range
        {
            get { return range; }
            set { range = value; }
        }

        public Vector3 Forward
        {
            get { return forward; }
            set { forward = value; }
        }

        public Vector3 PreviousPosition
        {
            get { return previousPosition; }
            set { previousPosition = value; }
        }

        public bool Belongs
        {
            get { return belongs; }
            set { belongs = value; }
        }

        public float ToDieTimer
        {
            get { return toDieTimer; }
            set
            {
                toDieTimer = value;
                if (toDieTimer > DIE_DELAY)
                    isAlive = false;
            }
        }

        public Vector3 Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        public float DistanceToDestination
        {
            get
            {
                float distance = 0f;
                if (path != null)
                {
                    distance += Vector3.Distance(Position, lastCheckpoint.Position);
                    for (int i = path.Checkpoints.IndexOf(lastCheckpoint);
                        i < path.Checkpoints.Count; i++)
                    {
                        if (i + 1 < path.Checkpoints.Count)
                            distance += Vector3.Distance(path.Checkpoints[i].Position, path.GetNextCheckpoint(path.Checkpoints[i]).Position);
                    }
                }
                else distance = -1;
                return distance;
            }
        }

        public Vector2 HealthBarOffset
        {
            get { return healthBarOffset; }
            set { healthBarOffset = value; }
        }

        public PathCheckpoint NextCheckpoint
        {
            get { return nextCheckpoint; }
            set { nextCheckpoint = value; }
        }

        public int PointsPerKill
        {
            get { return pointsPerKill; }
            set { pointsPerKill = value; }
        }

        #endregion

        public DynamicModel(Model model, MapPath path) :
            base(model, path.Checkpoints[0].Position)
        {
            this.path = path;
            this.lastCheckpoint = path.Checkpoints[0];
            NextCheckpoint = path.GetNextCheckpoint(lastCheckpoint);
            if (nextCheckpoint != null)
            {
                lastCheckpoint = nextCheckpoint;
            }
        }

        public DynamicModel(Model model, Vector3 position)
            : base(model, position)
        {

        }

        public virtual void SetMoving(Vector3 destination)
        {
            if (destination != Vector3.Zero)
            {
                Rotation = RotateToFace(destination);
                IsMoving = true;
                Destination = destination;
            }
        }

        public virtual void MoveUnit()
        {
            float distance = Vector3.Distance(Position, Destination);
            distance = (distance < 1) ? 1 : distance;

            if (Position != Destination && Destination != Vector3.Zero)
            {
                Position = Vector3.Lerp(Position, Destination, speed / distance);

                if ((Math.Abs(Math.Abs(Position.X) - Math.Abs(Destination.X)) < speed))
                    Position = new Vector3(Destination.X, Position.Y, Position.Z);
                if ((Math.Abs(Math.Abs(Position.Z) - Math.Abs(Destination.Z)) < speed))
                    Position = new Vector3(Position.X, Position.Y, Destination.Z);
                if (Position.X == Destination.X && Position.Z == Destination.Z)
                    Position = new Vector3(Position.X, Destination.Y, Position.Z);
            }

            else if (Position == Destination)
            {
                //if the path is not null, then the unit needs to keep following it
                if (path != null)
                {
                    PathCheckpoint nextCheckpoint = path.GetNextCheckpoint(lastCheckpoint);
                    if (nextCheckpoint != null)
                    {
                        SetMoving(nextCheckpoint.Position);
                        lastCheckpoint = nextCheckpoint;
                    }
                    else
                    {
                        //if there are no more checkpoints, that means the edge of the map is reached,
                        //the unit doesn't need to move (and be alive) anymore
                        ReachedEndOfPath = true;
                        IsAlive = false;
                    }
                }
                else IsMoving = false;
            }
        }

        protected Matrix RotateToFace(Vector3 destination)
        {
            Matrix rot = Matrix.Identity;
            if (destination != Position)
            {
                Vector3 U = Vector3.Up;
                Vector3 D = (destination - Position);
                Vector3 Right = Vector3.Cross(U, D);
                Vector3.Normalize(ref Right, out Right);
                Vector3 Backwards = Vector3.Cross(Right, U);
                Vector3.Normalize(ref Backwards, out Backwards);
                Vector3 Up = Vector3.Cross(Backwards, Right);
                rot = new Matrix(Right.X, Right.Y, Right.Z, 0, Up.X, Up.Y, Up.Z, 0, Backwards.X, Backwards.Y, Backwards.Z, 0, 0, 0, 0, 1);
            } return rot;
        }

        public void UpdateSelectionCircle()
        {
            if (Position != previousPosition)
                CreateSelectionCircle(SelectionCircleAttributes);
        }

        //returns true if the unit is about to die, false otherwise
        public bool InflictDamage(float damage)
        {
            if (IsDying)
                return false;

            CurrentHP -= damage - (damage * shield);
            if (CurrentHP < 0.5f)
            {
                IsDying = true;
                return true;
            }
            return false;
        }

        #region Selecting Unit With Selection Rectangle
        public bool Intersects(Rectangle rec)
        {
            return rec.Contains((int)TwoDProjection.X, (int)TwoDProjection.Y);
        }
        #endregion

    }
}
