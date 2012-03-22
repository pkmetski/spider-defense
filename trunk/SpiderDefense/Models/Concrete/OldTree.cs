using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpiderDefense
{
    class OldTree : StaticModel
    {
        public OldTree(ContentManager content, Vector3 position)
            : base(content.Load<Model>(@"Models\OldTree"), position)
        {
            Initialize();
        }

        private void Initialize()
        {
            float scale = 2.0f;
            Scale = Matrix.CreateScale(scale);
            Selectable = false;
            SelectionCircleRadiusVariable = scale/1.5f;
        }

        public override string[] OnScreenInfo
        {
            get
            {
                string[] info = new string[1];
                info[0] = "An Old Tree";
                return info;
            }
        }
    }
}
