using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class RightClickFlag : StaticModel
    {
        private const float visibleTime = 60;
        private float currentTime = visibleTime;
        private bool draw;

        public float VisibleTime
        {
            get { return visibleTime; }
        }

        public float CurrentTime
        {
            get { return currentTime; }
            set { currentTime = value; }
        }

        public bool Draw
        {
            get { return draw; }
            set { draw = value; }
        }

        public RightClickFlag(ContentManager content)
            : base(content.Load<Model>(@"Models\Tower"))
        {
            Init();
        }

        private void Init()
        {
            Scale = Matrix.CreateScale(0.03f);
        }

        public void TickTimer()
        {
            currentTime++;
            if (currentTime > visibleTime)
                draw = false;
        }

        public void Reset()
        {
            currentTime = 0;
            draw = true;
        }
    }
}
